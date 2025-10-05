# Region Switching Setup Guide

## Overview

This guide explains how to set up the region switching mechanic in your farming game. The system allows players to switch between different regions (Tropical Monsoon, Semi-Arid Steppe, Temperate Continental) while maintaining the same global week/date and using region-specific NASA data.

## 🎯 **Why In-Place Switching?**

**Benefits:**
- **Faster Performance**: No scene loading delays
- **Shared Global State**: Maintains global week/date across regions
- **Better UX**: Seamless transitions
- **Concurrent Regions**: Can switch between regions instantly
- **NASA Data Integration**: Each region gets its own NASA data

## 🏗️ **System Architecture**

### **Components Created:**

1. **RegionSwitcherUI.cs** - UI component for region selection
2. **RegionDataManager.cs** - Manages region-specific data
3. **Enhanced EndlessGameRunner.cs** - Supports region switching
4. **Enhanced EndlessGameHUD.cs** - Handles region change events

## 🎮 **Setup Instructions**

### **Step 1: Create Region Switching UI**

**1. Create Region Switch Button:**
- **Right-click in Hierarchy** → **UI** → **Button**
- **Name it "RegionSwitchButton"**
- **Add Text component** with text "Switch Region"

**2. Create Region Menu Panel:**
- **Right-click in Hierarchy** → **UI** → **Panel**
- **Name it "RegionMenuPanel"**
- **Add 3 child buttons** for each region:
  - "TropicalMonsoonButton"
  - "SemiAridSteppeButton" 
  - "TemperateContinentalButton"
- **Add Close Button** for closing the menu

**3. Add RegionSwitcherUI Component:**
- **Select the RegionMenuPanel**
- **Add Component** → **RegionSwitcherUI**
- **Assign UI References:**
  - Region Switch Button → `regionSwitchButton`
  - Region Menu Panel → `regionMenuPanel`
  - Region Buttons → `regionButtons` (array)
  - Region Button Texts → `regionButtonTexts` (array)
  - Current Region Text → `currentRegionText`

### **Step 2: Configure Region Data**

**1. Set Available Regions:**
```csharp
// In RegionSwitcherUI Inspector
Available Regions: [TropicalMonsoon, SemiAridSteppe, TemperateContinental]
Region Display Names: ["Tropical Monsoon", "Semi-Arid Steppe", "Temperate Continental"]
```

**2. Assign Region Sprites (Optional):**
- **Create region map sprites** for visual representation
- **Assign to Region Sprites array** in RegionSwitcherUI

### **Step 3: Connect Game References**

**1. Assign GameRunner:**
- **Drag EndlessGameRunner** to `gameRunner` field in RegionSwitcherUI
- **Drag EndlessGameHUD** to `gameHUD` field in RegionSwitcherUI

**2. Configure NASA Data:**
- **Ensure NasaDataConfig** is assigned to EndlessGameRunner
- **Set start date** in NasaDataConfig (e.g., March 1, 2024)
- **Enable/disable real NASA data** as needed

## 🎨 **UI Layout Suggestions**

### **Region Switch Button:**
- **Position**: Top-right corner of screen
- **Size**: 150x50 pixels
- **Text**: "Switch Region"
- **Style**: Prominent button with region icon

### **Region Menu Panel:**
- **Position**: Center of screen (overlay)
- **Size**: 400x300 pixels
- **Background**: Semi-transparent with region map
- **Layout**: 3 region buttons in a row

### **Region Buttons:**
- **Size**: 120x80 pixels each
- **Content**: Region name + small map preview
- **Highlighting**: Current region highlighted in green

## 🔧 **Technical Implementation**

### **Region Switching Flow:**

1. **Player clicks "Switch Region" button**
2. **RegionSwitcherUI opens region menu**
3. **Player selects new region**
4. **EndlessGameRunner.SwitchRegion() called**
5. **New EnhancedDataService created for new region**
6. **EndlessTurnManager reinitialized with new region data**
7. **UI updated to reflect new region**
8. **NASA data automatically switches to new region**

### **NASA Data Integration:**

**Each Region Gets Its Own Data:**
- **Tropical Monsoon**: Malaysia coordinates (1.5°N, 110.3°E)
- **Semi-Arid Steppe**: Tanzania coordinates (-6.2°N, 35.7°E)  
- **Temperate Continental**: Hungary coordinates (47.5°N, 21.6°E)

**Data Sources:**
- **IMERG**: Region-specific precipitation data
- **POWER**: Region-specific evapotranspiration data
- **MODIS**: Region-specific NDVI data

## 🎮 **Player Experience**

### **Region Switching:**
1. **Click "Switch Region" button**
2. **See 3 region options with maps**
3. **Click desired region**
4. **Game instantly switches to new region**
5. **Same week/date maintained**
6. **New environmental conditions applied**

### **Visual Feedback:**
- **Current region highlighted** in green
- **Region name displayed** in HUD
- **Data source indicator** (NASA vs RNG)
- **Smooth transitions** between regions

## 🧪 **Testing the System**

### **Test Region Switching:**
1. **Run the game**
2. **Click "Switch Region" button**
3. **Select different regions**
4. **Verify NASA data changes** (check console logs)
5. **Verify week/date consistency** across regions

### **Debug Information:**
```
EndlessGameRunner: Switching from TropicalMonsoon to SemiAridSteppe
EndlessGameRunner: Switched to SemiAridSteppe
EndlessGameRunner: Data Source: NASA Satellite Data
EndlessGameRunner: Using Real Data: True
```

## 🚀 **Advanced Features**

### **Concurrent Region State:**
- **Each region maintains its own state**
- **Plots, crops, and progress saved per region**
- **Global week/date shared across regions**
- **Region-specific NASA data**

### **Region-Specific Content:**
- **Different crops available per region**
- **Region-specific environmental challenges**
- **Unique farming strategies per region**
- **Educational content about regional agriculture**

## 📋 **Troubleshooting**

### **Common Issues:**

**1. Region Not Switching:**
- Check if EndlessGameRunner is assigned to RegionSwitcherUI
- Verify NasaDataConfig is properly configured
- Check console for error messages

**2. NASA Data Not Updating:**
- Ensure NasaDataConfig has correct start date
- Verify NASA data is enabled in config
- Check network connectivity for real data

**3. UI Not Updating:**
- Verify EndlessGameHUD is assigned to RegionSwitcherUI
- Check if OnRegionChanged method is being called
- Ensure UI elements are properly assigned

### **Debug Commands:**
```csharp
// Test region switching programmatically
[ContextMenu("Test Switch to Tropical Monsoon")]
public void TestSwitchToTropicalMonsoon() { ... }

[ContextMenu("Test Switch to Semi-Arid Steppe")]  
public void TestSwitchToSemiAridSteppe() { ... }
```

## 🎯 **Benefits of This System**

**Educational Value:**
- **Players learn about different climates**
- **Understand regional agriculture challenges**
- **Experience authentic environmental data**

**Gameplay Depth:**
- **Multiple farming strategies**
- **Region-specific challenges**
- **Long-term progression across regions**

**Technical Excellence:**
- **Seamless region switching**
- **Real NASA satellite data**
- **Efficient memory usage**
- **Scalable architecture**

The region switching system is now fully implemented and ready to provide an engaging, educational farming experience across multiple global regions!
