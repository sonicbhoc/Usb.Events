#include <CoreFoundation/CoreFoundation.h>

#include <IOKit/IOKitLib.h>
#include <IOKit/IOMessage.h>
#include <IOKit/IOCFPlugIn.h>
#include <IOKit/usb/IOUSBLib.h>

#include <signal.h>
#include <stdio.h>
#include <stdlib.h>

typedef struct UsbDeviceData
{
    char DeviceName[255];
    char DeviceSystemPath[255];
    char Product[255];
    char ProductDescription[255];
    char ProductID[255];
    char SerialNumber[255];
    char Vendor[255];
    char VendorDescription[255];
    char VendorID[255];
} UsbDeviceData;

UsbDeviceData usbDevice;

static const struct UsbDeviceData empty;

typedef void (*WatcherCallback)(UsbDeviceData usbDevice);
WatcherCallback InsertedCallback;
WatcherCallback RemovedCallback;

typedef void (*MessageCallback)(const char* message);
MessageCallback Message;

static IONotificationPortRef notificationPort;

void print_cfstringref(const char* prefix, CFStringRef cfVal)
{
	char* cVal = malloc(CFStringGetLength(cfVal) * sizeof(char));

	if (!cVal)
	{
		return;
	}

	if (CFStringGetCString(cfVal, cVal, CFStringGetLength(cfVal) + 1, kCFStringEncodingASCII))
	{
		printf("%s %s\n", prefix, cVal);
	}

	free(cVal);
}

void print_cfnumberref(const char* prefix, CFNumberRef cfVal)
{
	int result;

	if (CFNumberGetValue(cfVal, kCFNumberSInt32Type, &result))
	{
		printf("%s %i\n", prefix, result);
	}
}

// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

void scanUsbMassStorage(const char* syspath, MessageCallback message)
{
	CFMutableDictionaryRef matchingDictionary = IOServiceMatching(kIOUSBInterfaceClassName);

	// now specify class and subclass to iterate only through USB mass storage devices:
	CFNumberRef cfValue;
	SInt32 deviceClassNum = kUSBMassStorageInterfaceClass;
	cfValue = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &deviceClassNum);
	CFDictionaryAddValue(matchingDictionary, CFSTR(kUSBInterfaceClass), cfValue);
	CFRelease(cfValue);

	// NOTE: if you will specify only device class and will not specify subclass, it will return an empty iterator, and I don't know how to say that we need any subclass. 
	// BUT: all the devices I've check had kUSBMassStorageSCSISubClass
	SInt32 deviceSubClassNum = kUSBMassStorageSCSISubClass;
	cfValue = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &deviceSubClassNum);
	CFDictionaryAddValue(matchingDictionary, CFSTR(kUSBInterfaceSubClass), cfValue);
	CFRelease(cfValue);

	io_iterator_t foundIterator = 0;
	io_service_t usbInterface;
	IOServiceGetMatchingServices(kIOMasterPortDefault, matchingDictionary, &foundIterator);

	char* cVal;
	int found = 0;

	// iterate through USB mass storage devices
	for (usbInterface = IOIteratorNext(foundIterator); usbInterface; usbInterface = IOIteratorNext(foundIterator))
	{
		CFStringRef bsdName = (CFStringRef)IORegistryEntrySearchCFProperty(usbInterface,
			kIOServicePlane,
			CFSTR(kIOBSDNameKey),
			kCFAllocatorDefault,
			kIORegistryIterateRecursively);

		if (bsdName)
		{
			cVal = malloc(CFStringGetLength(bsdName) * sizeof(char));
			if (cVal)
			{
				if (CFStringGetCString(bsdName, cVal, CFStringGetLength(bsdName) + 1, kCFStringEncodingASCII))
				{
					if (strcmp(cVal, syspath) == 0)
					{
						char* mountPath = getMountPathByBSDName(cVal);

						if (mountPath)
						{
							found = 1;
							message(mountPath);
						}

						break;
					}
				}

				free(cVal);
			}
		}
	}

	if (!found)
		message("");
}

char* getMountPathByBSDName(char* bsdName)
{
	DASessionRef session = DASessionCreate(kCFAllocatorDefault);
	if (!session)
	{
		return NULL;
	}

	char buf[1024];
	char* cVal;
	int found = 0;

	CFDictionaryRef matchingDictionary = IOBSDNameMatching(kIOMasterPortDefault, 0, bsdName);
	io_iterator_t it;
	IOServiceGetMatchingServices(kIOMasterPortDefault, matchingDictionary, &it);
	io_object_t service;
	while ((service = IOIteratorNext(it)))
	{
		io_iterator_t children;
		io_registry_entry_t child;

		IORegistryEntryGetChildIterator(service, kIOServicePlane, &children);
		while ((child = IOIteratorNext(children)))
		{
			CFStringRef bsdNameChild = (CFStringRef)IORegistryEntrySearchCFProperty(child,
				kIOServicePlane,
				CFSTR(kIOBSDNameKey),
				kCFAllocatorDefault,
				kIORegistryIterateRecursively);

			if (bsdNameChild)
			{
				cVal = malloc(CFStringGetLength(bsdNameChild) * sizeof(char));
				if (cVal)
				{
					if (CFStringGetCString(bsdNameChild, cVal, CFStringGetLength(bsdNameChild) + 1, kCFStringEncodingASCII))
					{
						found = 1;

						// Copy / Paste --->
						DADiskRef disk = DADiskCreateFromBSDName(kCFAllocatorDefault, session, cVal);
						if (disk)
						{
							CFDictionaryRef diskInfo = DADiskCopyDescription(disk);
							if (diskInfo)
							{
								CFURLRef fspath = (CFURLRef)CFDictionaryGetValue(diskInfo, kDADiskDescriptionVolumePathKey);
								if (CFURLGetFileSystemRepresentation(fspath, false, (UInt8*)buf, 1024))
								{
									// for now, return the first found partition

									CFRelease(diskInfo);
									CFRelease(disk);
									CFRelease(session);
									free(cVal);

									return buf;
								}

								CFRelease(diskInfo);
							}

							CFRelease(disk);
						}
						// <--- Copy / Paste
					}

					free(cVal);
				}
			}
		}
	}

	/*
	The device could get name 'disk1s1, or just 'disk1'.
	In first case, the original bsd name would be 'disk1', and the child bsd name would be 'disk1s1'.
	In second case, there would be no child bsd names, but the original one is valid for further work (obtaining various properties).
	/**/

	if (!found)
	{
		// Copy / Paste --->
		DADiskRef disk = DADiskCreateFromBSDName(kCFAllocatorDefault, session, bsdName);
		if (disk)
		{
			CFDictionaryRef diskInfo = DADiskCopyDescription(disk);
			if (diskInfo)
			{
				CFURLRef fspath = (CFURLRef)CFDictionaryGetValue(diskInfo, kDADiskDescriptionVolumePathKey);
				if (CFURLGetFileSystemRepresentation(fspath, false, (UInt8*)buf, 1024))
				{
					// for now, return the first found partition

					CFRelease(diskInfo);
					CFRelease(disk);
					CFRelease(session);

					return buf;
				}

				CFRelease(diskInfo);
			}

			CFRelease(disk);
		}
		// <--- Copy / Paste
	}

	CFRelease(session);
	return NULL;
}

// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

void get_usb_device_info(io_service_t device, int newdev)
{
	io_name_t devicename;
	io_name_t devicepath;
	io_name_t classname;

	char* cVal;
	int result;

	if (IORegistryEntryGetName(device, devicename) != KERN_SUCCESS)
	{
		fprintf(stderr, "%s unknown device\n", newdev ? "added" : " removed");
		return;
	}

	usbDevice = empty;

	printf("USB device %s: %s\n", newdev ? "FOUND" : "REMOVED", devicename);

	strcpy(usbDevice.DeviceName, devicename);

	if (IORegistryEntryGetPath(device, kIOServicePlane, devicepath) == KERN_SUCCESS)
	{
		printf("\tDevice path: %s\n", devicepath);

		strcpy(usbDevice.DeviceSystemPath, devicepath);
	}

	if (IOObjectGetClass(device, classname) == KERN_SUCCESS)
	{
		printf("\tDevice class name: %s\n", classname);
	}

	CFStringRef bsdName = (CFStringRef)IORegistryEntrySearchCFProperty(device,
		kIOServicePlane,
		CFSTR(kIOBSDNameKey),
		kCFAllocatorDefault,
		kIORegistryIterateRecursively);

	if (bsdName)
	{
		cVal = malloc(CFStringGetLength(bsdName) * sizeof(char));
		if (cVal)
		{
			if (CFStringGetCString(bsdName, cVal, CFStringGetLength(bsdName) + 1, kCFStringEncodingASCII))
			{
				strcpy(usbDevice.DeviceSystemPath, cVal);

				char* mountPath = getMountPathByBSDName(cVal);

				if (mountPath)
				{
					Message(mountPath);
				}
			}

			free(cVal);
		}
	}

	CFStringRef vendorname = (CFStringRef)IORegistryEntrySearchCFProperty(device
		, kIOServicePlane
		, CFSTR("USB Vendor Name")
		, NULL
		, kIORegistryIterateRecursively | kIORegistryIterateParents);

	if (vendorname)
	{
		print_cfstringref("\tDevice vendor name:", vendorname);

		cVal = malloc(CFStringGetLength(vendorname) * sizeof(char));
		if (cVal)
		{
			if (CFStringGetCString(vendorname, cVal, CFStringGetLength(vendorname) + 1, kCFStringEncodingASCII))
			{
				strcpy(usbDevice.Vendor, cVal);
				strcpy(usbDevice.VendorDescription, cVal);
			}

			free(cVal);
		}
	}

	CFNumberRef vendorId = (CFNumberRef)IORegistryEntrySearchCFProperty(device
		, kIOServicePlane
		, CFSTR("idVendor")
		, NULL
		, kIORegistryIterateRecursively | kIORegistryIterateParents);

	if (vendorId)
	{
		print_cfnumberref("\tVendor id:", vendorId);

		if (CFNumberGetValue(vendorId, kCFNumberSInt32Type, &result))
		{
			sprintf(usbDevice.VendorID, "%d", result);
		}
	}

	CFStringRef productname = (CFStringRef)IORegistryEntrySearchCFProperty(device
		, kIOServicePlane
		, CFSTR("USB Product Name")
		, NULL
		, kIORegistryIterateRecursively | kIORegistryIterateParents);

	if (productname)
	{
		print_cfstringref("\tDevice product name:", productname);

		cVal = malloc(CFStringGetLength(productname) * sizeof(char));
		if (cVal)
		{
			if (CFStringGetCString(productname, cVal, CFStringGetLength(productname) + 1, kCFStringEncodingASCII))
			{
				strcpy(usbDevice.Product, cVal);
				strcpy(usbDevice.ProductDescription, cVal);
			}

			free(cVal);
		}
	}

	CFNumberRef productId = (CFNumberRef)IORegistryEntrySearchCFProperty(device
		, kIOServicePlane
		, CFSTR("idProduct")
		, NULL
		, kIORegistryIterateRecursively | kIORegistryIterateParents);

	if (productId)
	{
		print_cfnumberref("\tProduct id:", productId);

		if (CFNumberGetValue(productId, kCFNumberSInt32Type, &result))
		{
			sprintf(usbDevice.ProductID, "%d", result);
		}
	}

	CFStringRef serialnumber = (CFStringRef)IORegistryEntrySearchCFProperty(device
		, kIOServicePlane
		, CFSTR("USB Serial Number")
		, NULL
		, kIORegistryIterateRecursively | kIORegistryIterateParents);

	if (serialnumber)
	{
		print_cfstringref("\tDevice serial number:", serialnumber);

		cVal = malloc(CFStringGetLength(serialnumber) * sizeof(char));
		if (cVal)
		{
			if (CFStringGetCString(serialnumber, cVal, CFStringGetLength(serialnumber) + 1, kCFStringEncodingASCII))
			{
				strcpy(usbDevice.SerialNumber, cVal);
			}

			free(cVal);
		}
	}

	printf("\n");

	if (newdev)
	{
		InsertedCallback(usbDevice);
	}
	else
	{
		RemovedCallback(usbDevice);
	}
}

void iterate_usb_devices(io_iterator_t iterator, int newdev)
{
	io_service_t usbDevice;

	while ((usbDevice = IOIteratorNext(iterator)))
	{
		get_usb_device_info(usbDevice, newdev);
		IOObjectRelease(usbDevice);
	}
}

void usb_device_added(void* refcon, io_iterator_t iterator)
{
	iterate_usb_devices(iterator, 1);
}

void usb_device_removed(void* refcon, io_iterator_t iterator)
{
	iterate_usb_devices(iterator, 0);
}

void init_notifier()
{
	notificationPort = IONotificationPortCreate(kIOMasterPortDefault);
	CFRunLoopAddSource(CFRunLoopGetCurrent(), IONotificationPortGetRunLoopSource(notificationPort), kCFRunLoopDefaultMode);
	printf("init_notifier ok\n");
}

void configure_and_start_notifier()
{
	printf("Starting notifier\n");
	CFMutableDictionaryRef matchDict = (CFMutableDictionaryRef)CFRetain(IOServiceMatching(kIOUSBDeviceClassName));

	if (!matchDict)
	{
		fprintf(stderr, "Failed to create matching dictionary for kIOUSBDeviceClassName\n");
		return;
	}

	kern_return_t addResult;

	io_iterator_t deviceAddedIter;
	addResult = IOServiceAddMatchingNotification(notificationPort, kIOMatchedNotification, matchDict, usb_device_added, NULL, &deviceAddedIter);

	if (addResult != KERN_SUCCESS)
	{
		fprintf(stderr, "IOServiceAddMatchingNotification failed for kIOMatchedNotification\n");
		return;
	}

	usb_device_added(NULL, deviceAddedIter);

	io_iterator_t deviceRemovedIter;
	addResult = IOServiceAddMatchingNotification(notificationPort, kIOTerminatedNotification, matchDict, usb_device_removed, NULL, &deviceRemovedIter);

	if (addResult != KERN_SUCCESS)
	{
		fprintf(stderr, "IOServiceAddMatchingNotification failed for kIOTerminatedNotification\n");
		return;
	}

	usb_device_removed(NULL, deviceRemovedIter);

	CFRunLoopRun();
}

void deinit_notifier()
{
	CFRunLoopRemoveSource(CFRunLoopGetCurrent(), IONotificationPortGetRunLoopSource(notificationPort), kCFRunLoopDefaultMode);
	IONotificationPortDestroy(notificationPort);
	printf("deinit_notifier ok\n");
}

void signal_handler(int signum)
{
	printf("\ngot signal, signnum=%i  stopping current RunLoop\n", signum);
	CFRunLoopStop(CFRunLoopGetCurrent());
}

void init_signal_handler()
{
	signal(SIGINT, signal_handler);
	signal(SIGQUIT, signal_handler);
	signal(SIGTERM, signal_handler);
}

#ifdef __cplusplus
extern "C" {
#endif

void StartMacWatcher(WatcherCallback insertedCallback, WatcherCallback removedCallback, MessageCallback message)
{
	InsertedCallback = insertedCallback;
	RemovedCallback = removedCallback;
	Message = message;

	//init_signal_handler();
	init_notifier();
	configure_and_start_notifier();
	deinit_notifier();
}

#ifdef __cplusplus
}
#endif