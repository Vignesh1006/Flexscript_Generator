# Converting FlexSim Code Generator to Android APK

## Overview
This guide explains how to convert the HTML/JavaScript FlexSim Code Generator into an Android APK app.

---

## Method 1: Using Apache Cordova (Recommended)

### Prerequisites
- Node.js installed
- Android Studio installed
- Java JDK installed

### Steps

#### 1. Install Cordova
```bash
npm install -g cordova
```

#### 2. Create Cordova Project
```bash
cordova create FlexSimCodeGen com.flexsim.codegen FlexSimCodeGenerator
cd FlexSimCodeGen
```

#### 3. Add Android Platform
```bash
cordova platform add android
```

#### 4. Copy Your HTML File
Replace the contents of `www/index.html` with the FlexSim Code Generator HTML file.

#### 5. Update config.xml
Edit `config.xml` to configure your app:

```xml
<?xml version='1.0' encoding='utf-8'?>
<widget id="com.flexsim.codegen" version="1.0.0" xmlns="http://www.w3.org/ns/widgets">
    <name>FlexSim Code Generator</name>
    <description>
        Generate FlexSim code snippets for specific scenarios
    </description>
    <author email="your@email.com" href="http://yourwebsite.com">
        Your Name
    </author>
    <content src="index.html" />
    <access origin="*" />
    <allow-intent href="http://*/*" />
    <allow-intent href="https://*/*" />
    <preference name="DisallowOverscroll" value="true" />
    <preference name="android-minSdkVersion" value="21" />
    <preference name="android-targetSdkVersion" value="30" />
</widget>
```

#### 6. Build APK
```bash
# Debug APK (for testing)
cordova build android

# Release APK (for distribution)
cordova build android --release
```

#### 7. Find Your APK
The APK will be located at:
```
platforms/android/app/build/outputs/apk/debug/app-debug.apk
```

---

## Method 2: Using Capacitor (Modern Alternative)

### Steps

#### 1. Install Capacitor
```bash
npm install -g @capacitor/cli @capacitor/core @capacitor/android
```

#### 2. Initialize Capacitor
```bash
mkdir FlexSimCodeGen
cd FlexSimCodeGen
npm init -y
npm install @capacitor/core @capacitor/cli @capacitor/android
npx cap init
```

#### 3. Add Android Platform
```bash
npx cap add android
```

#### 4. Copy Your HTML
Copy the HTML file to the `www` folder (create if doesn't exist)

#### 5. Sync and Build
```bash
npx cap sync
npx cap open android
```

This opens Android Studio where you can build the APK.

---

## Method 3: Using Android Studio WebView (Manual)

### Steps

#### 1. Create New Android Project
- Open Android Studio
- Create new "Empty Activity" project
- Choose API 21+ (Android 5.0)

#### 2. Enable Internet Permission
Edit `AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

#### 3. Create Assets Folder
- Right-click on `app` → New → Folder → Assets Folder
- Copy your HTML file to `app/src/main/assets/`

#### 4. Modify MainActivity
```java
package com.flexsim.codegenerator;

import android.os.Bundle;
import android.webkit.WebView;
import android.webkit.WebSettings;
import android.webkit.WebViewClient;
import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {
    private WebView webView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        webView = findViewById(R.id.webview);
        WebSettings webSettings = webView.getSettings();
        webSettings.setJavaScriptEnabled(true);
        webSettings.setDomStorageEnabled(true);
        
        webView.setWebViewClient(new WebViewClient());
        webView.loadUrl("file:///android_asset/flexsim-code-generator.html");
    }

    @Override
    public void onBackPressed() {
        if (webView.canGoBack()) {
            webView.goBack();
        } else {
            super.onBackPressed();
        }
    }
}
```

#### 5. Update Layout
Edit `activity_main.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:layout_width="match_parent"
    android:layout_height="match_parent"
    android:orientation="vertical">

    <WebView
        android:id="@+id/webview"
        android:layout_width="match_parent"
        android:layout_height="match_parent" />
</LinearLayout>
```

#### 6. Build APK
- Build → Build Bundle(s) / APK(s) → Build APK(s)

---

## Method 4: Online Converter Tools (Easiest - No Coding)

### Using Website2APK Builder
1. Visit: https://www.websitetoapk.com/ or https://appsgeyser.com/
2. Upload your HTML file or provide URL
3. Configure app settings (name, icon, colors)
4. Generate APK
5. Download and install

### Using PWABuilder
1. Visit: https://www.pwabuilder.com/
2. Host your HTML file online
3. Enter the URL
4. Generate Android package
5. Download APK

---

## Method 5: Using PhoneGap Build (Cloud-based)

### Steps
1. Sign up at https://build.phonegap.com/
2. Create new app
3. Upload your HTML file in a zip with this structure:
```
myapp.zip
├── www/
│   └── index.html
└── config.xml
```
4. PhoneGap Build will create APK
5. Download the APK

---

## Signing Your APK for Google Play Store

### Generate Keystore
```bash
keytool -genkey -v -keystore my-release-key.keystore -alias my-key-alias -keyalg RSA -keysize 2048 -validity 10000
```

### Sign APK
```bash
jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore my-release-key.keystore app-release-unsigned.apk my-key-alias
```

### Align APK
```bash
zipalign -v 4 app-release-unsigned.apk FlexSimCodeGenerator.apk
```

---

## Enhanced Features for Mobile App

### Add to HTML (Optional Enhancements)

#### 1. Viewport Meta Tag (Already included)
```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```

#### 2. Add PWA Manifest
Create `manifest.json`:
```json
{
  "name": "FlexSim Code Generator",
  "short_name": "FlexSim Gen",
  "description": "Generate FlexSim code for specific scenarios",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#667eea",
  "theme_color": "#667eea",
  "orientation": "portrait",
  "icons": [
    {
      "src": "icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ]
}
```

#### 3. Add Touch Icons
```html
<link rel="apple-touch-icon" sizes="180x180" href="apple-touch-icon.png">
<link rel="icon" type="image/png" sizes="32x32" href="favicon-32x32.png">
```

#### 4. Add Share Functionality
```javascript
function shareCode() {
    const codeText = document.getElementById('codeOutput').innerText;
    
    if (navigator.share) {
        navigator.share({
            title: 'FlexSim Code',
            text: codeText
        }).then(() => {
            console.log('Thanks for sharing!');
        }).catch(console.error);
    } else {
        // Fallback - copy to clipboard
        copyCode();
    }
}
```

#### 5. Add Local Storage for Favorites
```javascript
function saveFavorite() {
    const scenarioType = document.getElementById('scenarioType').value;
    const favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    
    favorites.push({
        type: scenarioType,
        timestamp: Date.now(),
        code: document.getElementById('codeOutput').innerText
    });
    
    localStorage.setItem('favorites', JSON.stringify(favorites));
    alert('Saved to favorites!');
}

function loadFavorites() {
    const favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    // Display favorites list
}
```

---

## Testing Your APK

### Install on Android Device
```bash
adb install app-debug.apk
```

### Enable USB Debugging
1. Go to Settings → About Phone
2. Tap "Build Number" 7 times
3. Go to Settings → Developer Options
4. Enable "USB Debugging"

### View Logs
```bash
adb logcat
```

---

## Recommended Workflow for Your Use Case

**Best Option: Apache Cordova** (Method 1)

Why?
- ✅ Easy to set up
- ✅ Cross-platform (iOS + Android)
- ✅ Good documentation
- ✅ Supports plugins for extra features
- ✅ No need to learn native Android development

### Quick Start Command
```bash
# Install Cordova
npm install -g cordova

# Create project
cordova create FlexSimApp com.flexsim.codegen "FlexSim Code Generator"
cd FlexSimApp

# Copy your HTML to www/index.html
# (Replace the default index.html)

# Add Android platform
cordova platform add android

# Build APK
cordova build android

# Your APK is ready at:
# platforms/android/app/build/outputs/apk/debug/app-debug.apk
```

---

## App Store Submission Checklist

### For Google Play Store
- [ ] App icon (512x512 PNG)
- [ ] Feature graphic (1024x500 PNG)
- [ ] Screenshots (at least 2, max 8)
- [ ] App description (up to 4000 characters)
- [ ] Privacy policy URL (required)
- [ ] Signed APK or Android App Bundle
- [ ] Developer account ($25 one-time fee)

### Asset Requirements
- App icon: 512x512 px, PNG
- Feature graphic: 1024x500 px, PNG
- Screenshots: 16:9 or 9:16 ratio
- Promo video (optional): YouTube link

---

## Troubleshooting Common Issues

### Issue 1: JavaScript not working
**Solution**: Ensure WebView JavaScript is enabled
```java
webSettings.setJavaScriptEnabled(true);
```

### Issue 2: Assets not loading
**Solution**: Check file path
```java
webView.loadUrl("file:///android_asset/index.html");
```

### Issue 3: Clipboard not working
**Solution**: Add permission
```xml
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
```

### Issue 4: Build fails
**Solution**: Check Android SDK path
```bash
export ANDROID_HOME=/path/to/android/sdk
```

---

## Cost Comparison

| Method | Cost | Difficulty | Time |
|--------|------|------------|------|
| Cordova | Free | Easy | 30 min |
| Capacitor | Free | Easy | 30 min |
| Android Studio | Free | Medium | 2-3 hours |
| Online Tools | Free-$10 | Very Easy | 10 min |
| PhoneGap Build | $10/month | Easy | 20 min |

---

## Additional Resources

- Apache Cordova: https://cordova.apache.org/
- Capacitor: https://capacitorjs.com/
- Android Studio: https://developer.android.com/studio
- PhoneGap Build: https://build.phonegap.com/
- PWA Builder: https://www.pwabuilder.com/

---

## Summary

**Recommended Path for Beginners:**
1. Use **Apache Cordova** (easiest, most documented)
2. Copy HTML file to Cordova project
3. Build APK with single command
4. Test on Android device
5. Submit to Google Play Store (optional)

**For Quick Testing:**
Use online converter like **Website2APK** or **AppsGeyser** - no coding required!

**For Professional App:**
Use **Android Studio WebView** approach for full control and better performance.
