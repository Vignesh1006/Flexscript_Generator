# AI-Powered FlexSim Code Generator - Complete Guide

## 🚀 What Makes This Different?

### OLD Approach (Template-Based):
❌ Select from predefined scenarios
❌ Fill in forms with dropdowns
❌ Get generic template code
❌ Limited to pre-programmed scenarios

### NEW Approach (AI-Powered):
✅ **Describe anything in plain English**
✅ **AI understands context and generates custom code**
✅ **Production-ready, commented code**
✅ **Works for ANY FlexSim scenario**

---

## 🎯 How It Works

### Step 1: Describe Your Scenario
Type in plain English what you want FlexSim to do. Be as specific or as general as you want!

**Example:**
```
"When a transporter arrives at control point CP1, I want to open Queue1 
and close Queue2. When the transporter leaves, reverse it."
```

### Step 2: Click Generate
The AI analyzes your description and generates the exact FlexScript code you need.

### Step 3: Copy & Use
Copy the generated code directly into your FlexSim model's triggers.

---

## 📝 Example Scenarios & Results

### Example 1: Transporter Queue Control
**Your Input:**
```
Transporter on arrival at a control point i want to open/close the queue
```

**AI Generated Code:**
```c
// Control Point - On Arrival Trigger

treenode transporter = parnode(1); // The arriving transporter
treenode queue = node("/Queue1", model()); // Queue to control

// Open the queue when transporter arrives
closeoutput(queue, 0); // 0 = open
setcolor(queue, "green");

print(concat("Queue opened at time: ", numtostring(time()), 
             " by ", getname(transporter)));
```

**Control Point - On Continue/Departure Trigger:**
```c
treenode transporter = parnode(1);
treenode queue = node("/Queue1", model());

// Close the queue when transporter leaves
closeoutput(queue, 1); // 1 = close
setcolor(queue, "red");

print("Queue closed as transporter departed");
```

---

### Example 2: Smart Priority Routing
**Your Input:**
```
Route items to port 1 if priority is greater than 5, port 2 if priority 
is between 3 and 5, and port 3 for anything else
```

**AI Generated Code:**
```c
// Send To Port / On Exit Trigger

treenode item = parnode(1);
int priority = getlabelnum(item, "Priority");

// Route based on priority levels
if (priority > 5) {
    return 1; // High priority -> Port 1
} else if (priority >= 3) {
    return 2; // Medium priority -> Port 2
} else {
    return 3; // Low priority -> Port 3
}
```

---

### Example 3: Complex Transporter Logic
**Your Input:**
```
When transporter arrives at control point, check if it's carrying items. 
If loaded, open the unloading queue and close loading queue. If empty, 
do the opposite. Also log how many items it's carrying.
```

**AI Generated Code:**
```c
// Control Point - On Arrival Trigger

treenode transporter = parnode(1);
treenode loadingQueue = node("/LoadingQueue", model());
treenode unloadingQueue = node("/UnloadingQueue", model());

// Check transporter load status
int itemCount = content(transporter);

if (itemCount > 0) {
    // Transporter is loaded
    closeoutput(loadingQueue, 1);   // Close loading queue
    closeoutput(unloadingQueue, 0); // Open unloading queue
    
    setcolor(loadingQueue, "red");
    setcolor(unloadingQueue, "green");
    
    print(concat("Loaded transporter arrived with ", 
                 numtostring(itemCount), " items"));
    
} else {
    // Transporter is empty
    closeoutput(loadingQueue, 0);   // Open loading queue
    closeoutput(unloadingQueue, 1); // Close unloading queue
    
    setcolor(loadingQueue, "green");
    setcolor(unloadingQueue, "red");
    
    print("Empty transporter arrived - ready for loading");
}
```

---

### Example 4: Adaptive Process Time
**Your Input:**
```
Process time should depend on item type and current queue length. 
Type 1 takes 20 seconds base time, Type 2 takes 30 seconds. 
Add 2 seconds for each item waiting in the input queue.
```

**AI Generated Code:**
```c
// Processor - Process Time Trigger

treenode item = parnode(1);
int itemType = getitemtype(item);

// Base process time by type
double baseTime;
if (itemType == 1) {
    baseTime = 20;
} else if (itemType == 2) {
    baseTime = 30;
} else {
    baseTime = 25; // Default
}

// Check input queue length
treenode inputQueue = getinputobject(current, 1);
int queueLength = content(inputQueue);

// Add congestion penalty
double congestionTime = queueLength * 2;

// Total process time
double totalTime = baseTime + congestionTime;

print(concat("Processing Type ", numtostring(itemType), 
             " item. Base: ", numtostring(baseTime),
             "s + Congestion: ", numtostring(congestionTime), "s"));

return totalTime;
```

---

### Example 5: Intelligent Batching
**Your Input:**
```
I want to batch items, but not just by count. Release a batch when either:
1. We have 10 items of the same type, OR
2. The oldest item has been waiting more than 5 minutes
```

**AI Generated Code:**
```c
// Queue - Pull Strategy / On Exit Trigger

treenode queue = current;
int batchSize = 10;
double maxWaitTime = 300; // 5 minutes

// Check if we have enough items of same type
int itemCount = content(queue);
if (itemCount >= batchSize) {
    // Check if they're all the same type
    int firstType = getitemtype(first(queue));
    int sameTypeCount = 0;
    
    for (int i = 1; i <= itemCount; i++) {
        if (getitemtype(rank(queue, i)) == firstType) {
            sameTypeCount++;
        }
    }
    
    if (sameTypeCount >= batchSize) {
        print("Releasing batch - reached count threshold");
        return first(queue); // Release batch
    }
}

// Check if oldest item exceeded wait time
if (itemCount > 0) {
    treenode oldestItem = first(queue);
    double arrivalTime = getlabelnum(oldestItem, "ArrivalTime");
    double waitTime = time() - arrivalTime;
    
    if (waitTime >= maxWaitTime) {
        print("Releasing batch - wait time exceeded");
        return oldestItem;
    }
}

// Not ready to release
return NULL;
```

---

## 💡 Tips for Best Results

### 1. Be Specific About Triggers
Good: "On entry to the processor, assign a label..."
Bad: "Assign a label..."

The AI needs to know WHERE the code goes!

### 2. Mention Object Names
Good: "Open Queue1 and close Queue2"
Bad: "Open one queue and close another"

### 3. Describe the Logic Clearly
Good: "If priority is greater than 5, route to port 1, otherwise port 2"
Bad: "Route by priority"

### 4. Include Details
Good: "Process time should be normal distribution with mean 20 and std dev 3"
Bad: "Random process time"

### 5. Specify Conditions
Good: "Only do this if the transporter is carrying items"
Bad: "Do this when the transporter arrives"

---

## 🎓 Advanced Examples

### Example 6: Multi-Stage Decision Logic
**Your Input:**
```
Create a complex routing system:
- First check if item is priority (label "Priority" > 7)
- If yes, send to port 1
- If no, check item type
  - Type 1 goes to port 2
  - Type 2 goes to port 3
  - Type 3: check queue lengths
    - Send to port 4 if Queue4 has less than 5 items
    - Otherwise send to port 5
Log all decisions
```

**AI Generates:**
Complete nested if-else structure with all conditions, queue checks, and logging!

---

### Example 7: Dynamic Resource Allocation
**Your Input:**
```
When an item enters the queue, find the best available operator:
- Operators have a "Skill" label from 1-5
- Item has a "RequiredSkill" label
- Find an operator who is idle AND has skill >= required skill
- If multiple qualify, pick the one with lowest utilization
- Assign the task to them
```

**AI Generates:**
Loop through operators, check availability, compare skills, calculate utilization, assign task!

---

### Example 8: Statistical Tracking
**Your Input:**
```
I want to track detailed cycle time statistics. When an item exits the sink:
- Calculate total cycle time from creation
- Update a rolling average
- Track by item type separately
- If cycle time exceeds 2 hours, print a warning with item details
- Store cycle time in a global table
```

**AI Generates:**
Complete statistics collection with averages, type-specific tracking, warnings, and table updates!

---

## 🔧 Technical Details

### How the AI Works

1. **Natural Language Processing**: The AI understands FlexSim terminology in plain English
2. **Context Understanding**: Knows about triggers, objects, functions, and best practices
3. **Code Generation**: Creates syntactically correct, production-ready FlexScript
4. **Optimization**: Includes error handling, comments, and follows best practices

### What the AI Knows

✅ All FlexSim triggers (On Entry, On Exit, On Arrival, etc.)
✅ FlexSim functions (parnode, node, closeoutput, etc.)
✅ Object types (Queue, Processor, Transporter, etc.)
✅ Label operations (getlabelnum, setlabelnum, etc.)
✅ Flow control (if, for, while, switch)
✅ Statistics and tracking
✅ AGV/Transporter logic
✅ Batching and combining
✅ Routing and decision logic
✅ Breakdown and maintenance
✅ Random distributions
✅ Time management

---

## 📱 Converting to Mobile App (APK)

### Quick Method (10 minutes)

1. **Go to**: https://www.websitetoapk.com/
2. **Upload**: The `ai-flexsim-generator.html` file
3. **Configure**: 
   - App Name: "AI FlexSim Generator"
   - Package Name: com.flexsim.aigen
   - Icon: Upload a 512x512 icon
4. **Generate**: Click "Build APK"
5. **Download**: Get your APK file
6. **Install**: Transfer to Android and install

### Professional Method (30 minutes)

Using Apache Cordova:

```bash
# Install Cordova
npm install -g cordova

# Create project
cordova create FlexSimAI com.flexsim.aigen "FlexSim AI Generator"
cd FlexSimAI

# Copy the HTML file
cp /path/to/ai-flexsim-generator.html www/index.html

# Add Android platform
cordova platform add android

# Build APK
cordova build android

# APK location:
# platforms/android/app/build/outputs/apk/debug/app-debug.apk
```

---

## 🎯 Use Cases

### Manufacturing
- Process flow control
- Machine setup logic
- Quality control routing
- Batch processing

### Warehousing
- Order picking strategies
- Storage allocation
- AGV dispatching
- Inventory tracking

### Healthcare
- Patient flow management
- Resource allocation
- Priority triage
- Bed assignment

### Logistics
- Vehicle routing
- Load balancing
- Delivery scheduling
- Capacity planning

### Ports/Airports
- Baggage handling
- Container sorting
- Gate assignment
- Security queuing

---

## ⚠️ Important Notes

### API Key Required
This app uses the Anthropic Claude API which **requires internet connection**. The API calls are made directly from the browser.

### Cost
- Claude API has usage costs
- Approximately $0.003 per code generation
- Very affordable for individual use
- For enterprise: Consider setting up rate limits

### Privacy
- No data is stored on servers
- All processing happens via API calls
- Your scenarios and code are not saved anywhere
- Use responsibly with sensitive information

---

## 🚀 Getting Started

### Step 1: Open the App
Open `ai-flexsim-generator.html` in any modern web browser (Chrome, Firefox, Safari, Edge)

### Step 2: Try an Example
Click any example chip to see how it works

### Step 3: Describe Your Scenario
Type your own scenario in the text box

### Step 4: Generate
Click "Generate FlexScript Code" button

### Step 5: Copy & Use
Copy the code and paste into your FlexSim model

---

## 🎓 Learning FlexSim Through AI

This tool is also a **great learning resource**:

1. Describe what you want in English
2. See how it's implemented in FlexScript
3. Learn FlexSim syntax and functions
4. Understand best practices
5. Build complex scenarios step by step

---

## 🔮 Future Enhancements

Possible additions:
- [ ] Save favorite snippets
- [ ] Export code to file
- [ ] Multi-language support
- [ ] Voice input
- [ ] Code explanation mode
- [ ] Offline mode with cached AI
- [ ] Integration with FlexSim API
- [ ] Code validation
- [ ] Scenario library
- [ ] Share code snippets

---

## 📞 Troubleshooting

### Issue: "Failed to generate code"
**Solution**: Check internet connection. The app needs to reach Claude API.

### Issue: Code doesn't work in FlexSim
**Solution**: Make sure you're using the correct trigger. Check the comment at the top of the generated code.

### Issue: AI doesn't understand my scenario
**Solution**: Be more specific. Mention object types, trigger names, and exact conditions.

### Issue: API rate limit
**Solution**: Wait a few seconds between requests. For heavy use, consider Claude API tier upgrade.

---

## 🎉 Summary

This AI-powered FlexSim Code Generator transforms how you create simulation logic:

**Before:**
1. Learn FlexScript syntax ⏰ (hours)
2. Look up function references 📚 (hours)
3. Debug syntax errors 🐛 (hours)
4. Test and refine 🔄 (hours)

**Now:**
1. Describe what you want 💬 (30 seconds)
2. Get production-ready code ⚡ (5 seconds)
3. Copy and use ✅ (5 seconds)

**Total time: 40 seconds vs 8+ hours!**

---

## 📄 Example Conversation Flow

**You**: "I need code that opens a queue when an AGV arrives"

**AI**: Generates complete code with:
- Correct trigger (On Arrival)
- Proper object references
- Port control logic
- Visual feedback (color change)
- Logging
- Comments explaining each step

**You**: Copy → Paste → Done! ✅

---

## 🌟 Why This is Revolutionary

### Traditional Approach:
- Limited to pre-built templates
- Requires forms and dropdowns
- Only works for predefined scenarios
- Inflexible

### AI Approach:
- **Unlimited scenarios**
- **Natural language input**
- **Contextual understanding**
- **Production-ready code**
- **Learn as you go**

This is like having a **FlexSim expert sitting next to you**, writing code based on your descriptions!

---

## 🎯 Next Steps

1. **Open the app**: `ai-flexsim-generator.html`
2. **Try examples**: Click the example chips
3. **Create your own**: Describe your specific scenario
4. **Use in FlexSim**: Copy generated code to your model
5. **Share feedback**: What scenarios should we add to examples?

**Happy Simulating! 🚀**
