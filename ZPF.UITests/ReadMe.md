![ZPF.UITests](Doc/iconGitHub.svg)

ZPF.UITests is a starter nuget for MSTest and Appium. It provides a simple and efficient way to set up and run UI tests for your applications. With ZPF.UITests, you can quickly create and execute tests that interact with your application's user interface, ensuring that your app works as expected across different devices and platforms.

# !!! under construction !!!

---   

# Install appium on MacOSX

Go to https://nodejs.org/
Download and install the latest stable version of node.js package in your mac

sudo chown -R $(whoami) /usr/local/lib/node_modules
sudo chown -R $(whoami) /usr/local/bin
       
npm install -g appium

## Install the Android SDK manually
https://learn.microsoft.com/en-us/java/openjdk/download#openjdk-21

For Rider on macOS with Microsoft OpenJDK, set the Java SDK path to:
/Library/Java/JavaVirtualMachines/microsoft-<version>.jdk/Contents/Home
 
sudo rm -rf ~/.npm/_cacache
npm cache verify
 
appium driver install mac2

appium driver install uiautomator2
