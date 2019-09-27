#import "DeepLinkAppDelegate.h"

// Makes sure your app controller delegate is the one that gets loaded.
IMPL_APP_CONTROLLER_SUBCLASS(DeepLinkAppDelegate)

@implementation DeepLinkAppDelegate

- (void) deepLinkIsAlive
{
	if (_lastURL)
	{
		const char *URLString = [_lastURL cStringUsingEncoding:NSASCIIStringEncoding];
    	UnitySendMessage("_DeepLinkReceiver", "URLOpened", URLString);
	}
}
- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
	_lastURL = url.absoluteString;
	const char *URLString = [url.absoluteString UTF8String];
    UnitySendMessage("_DeepLinkReceiver", "URLOpened", URLString);

	return [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
}
- (char *) deepLinkURL
{
	return [(_lastURL ? _lastURL : @"") UTF8String];
}
@end
