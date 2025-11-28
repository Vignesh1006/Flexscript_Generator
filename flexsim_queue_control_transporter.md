# FlexSim: Control Queue Open/Close on Transporter Arrival at Control Point

## Overview
This guide shows how to open or close specific queues when a transporter (AGV, Operator, etc.) arrives at or leaves a control point.

---

## Scenario 1: Basic Queue Control - Single Queue

### Setup:
- **Transporter**: AGV or Operator
- **Control Point**: CP1 (on AGV network or path)
- **Queue**: Queue1 (to be controlled)

### Method 1: Using Control Point "On Arrival" Trigger

#### Control Point - On Arrival Trigger
```c
// This code runs when any transporter arrives at this control point

treenode transporter = parnode(1); // The arriving transporter
treenode queue = node("/Queue1", model()); // Target queue to control

// Open the queue (allow items to leave)
closeoutput(queue, 0); // 0 = open output

// Optional: Add visual feedback
setcolor(queue, "green");

// Optional: Log the event
print(concat("Queue1 opened at time: ", numtostring(time()), 
             " by transporter: ", getname(transporter)));
```

#### Control Point - On Continue/Departure Trigger
```c
// This code runs when transporter leaves the control point

treenode transporter = parnode(1);
treenode queue = node("/Queue1", model());

// Close the queue (prevent items from leaving)
closeoutput(queue, 1); // 1 = close output

// Optional: Add visual feedback
setcolor(queue, "red");

// Optional: Log the event
print(concat("Queue1 closed at time: ", numtostring(time())));
```

---

## Scenario 2: Toggle Queue State (Open/Close on Each Arrival)

### Control Point - On Arrival Trigger
```c
// Toggle queue state each time transporter arrives

treenode transporter = parnode(1);
treenode queue = node("/Queue1", model());

// Check current state of queue
int currentState = getoutputstatus(queue); // 0 = open, 1 = closed

if(currentState == 0) {
    // Currently open, so close it
    closeoutput(queue, 1);
    setcolor(queue, "red");
    print("Queue1 CLOSED");
} else {
    // Currently closed, so open it
    closeoutput(queue, 0);
    setcolor(queue, "green");
    print("Queue1 OPENED");
}
```

---

## Scenario 3: Control Multiple Queues

### Control Point - On Arrival Trigger
```c
// Open/close multiple queues when transporter arrives

treenode transporter = parnode(1);

// Define all queues to control
treenode queue1 = node("/Queue1", model());
treenode queue2 = node("/Queue2", model());
treenode queue3 = node("/Queue3", model());

// Open Queue1 and Queue2
closeoutput(queue1, 0);
closeoutput(queue2, 0);
setcolor(queue1, "green");
setcolor(queue2, "green");

// Close Queue3
closeoutput(queue3, 1);
setcolor(queue3, "red");

print("Arrival: Queue1 & Queue2 OPENED, Queue3 CLOSED");
```

### Control Point - On Continue Trigger
```c
// Reverse the queue states when transporter leaves

treenode transporter = parnode(1);

treenode queue1 = node("/Queue1", model());
treenode queue2 = node("/Queue2", model());
treenode queue3 = node("/Queue3", model());

// Close Queue1 and Queue2
closeoutput(queue1, 1);
closeoutput(queue2, 1);
setcolor(queue1, "red");
setcolor(queue2, "red");

// Open Queue3
closeoutput(queue3, 0);
setcolor(queue3, "green");

print("Departure: Queue1 & Queue2 CLOSED, Queue3 OPENED");
```

---

## Scenario 4: Conditional Queue Control Based on Transporter Type

### Control Point - On Arrival Trigger
```c
// Different queue control based on which transporter arrives

treenode transporter = parnode(1);
int transporterType = getlabelnum(transporter, "Type");

treenode queue1 = node("/Queue1", model());
treenode queue2 = node("/Queue2", model());

if(transporterType == 1) {
    // Type 1 transporter opens Queue1
    closeoutput(queue1, 0);
    closeoutput(queue2, 1);
    print("Type 1 AGV: Queue1 OPENED, Queue2 CLOSED");
    
} else if(transporterType == 2) {
    // Type 2 transporter opens Queue2
    closeoutput(queue1, 1);
    closeoutput(queue2, 0);
    print("Type 2 AGV: Queue1 CLOSED, Queue2 OPENED");
    
} else {
    // Other types close both queues
    closeoutput(queue1, 1);
    closeoutput(queue2, 1);
    print("Other AGV: Both queues CLOSED");
}
```

---

## Scenario 5: Queue Control Based on Transporter Load Status

### Control Point - On Arrival Trigger
```c
// Control queues based on whether transporter is loaded or empty

treenode transporter = parnode(1);
treenode queue1 = node("/LoadingQueue", model());
treenode queue2 = node("/UnloadingQueue", model());

// Check if transporter is carrying items
int itemCount = content(transporter);

if(itemCount > 0) {
    // Transporter is loaded - open unloading queue
    closeoutput(queue1, 1); // Close loading queue
    closeoutput(queue2, 0); // Open unloading queue
    
    setcolor(queue1, "red");
    setcolor(queue2, "green");
    
    print(concat("Loaded transporter arrived - ", 
                 numtostring(itemCount), " items"));
    
} else {
    // Transporter is empty - open loading queue
    closeoutput(queue1, 0); // Open loading queue
    closeoutput(queue2, 1); // Close unloading queue
    
    setcolor(queue1, "green");
    setcolor(queue2, "red");
    
    print("Empty transporter arrived");
}
```

---

## Scenario 6: Time-Delayed Queue Control

### Control Point - On Arrival Trigger
```c
// Open queue immediately, but schedule it to close after delay

treenode transporter = parnode(1);
treenode queue = node("/Queue1", model());

// Open the queue immediately
closeoutput(queue, 0);
setcolor(queue, "green");
print("Queue1 OPENED immediately");

// Schedule queue to close after 60 seconds
double delayTime = 60; // seconds

// Create delayed event
delayedmsg(current, delayTime, 1, queue); // msgparam(1) = 1 for close action
```

### Control Point - On Message Trigger
```c
// This receives the delayed message to close the queue

int messageCode = msgparam(1);
treenode queue = msgparam(2);

if(messageCode == 1) {
    // Close the queue
    closeoutput(queue, 1);
    setcolor(queue, "red");
    print(concat("Queue1 CLOSED after delay at time: ", numtostring(time())));
}
```

---

## Scenario 7: Queue Control with Counter (Open After N Arrivals)

### Control Point - On Reset Trigger
```c
// Initialize counter
setlabelnum(current, "ArrivalCount", 0);
setlabelnum(current, "ThresholdCount", 5); // Open queue after 5 arrivals

// Close queue initially
treenode queue = node("/Queue1", model());
closeoutput(queue, 1);
setcolor(queue, "red");
```

### Control Point - On Arrival Trigger
```c
// Count arrivals and open queue after threshold reached

treenode transporter = parnode(1);
treenode queue = node("/Queue1", model());

// Increment counter
int count = getlabelnum(current, "ArrivalCount") + 1;
setlabelnum(current, "ArrivalCount", count);

int threshold = getlabelnum(current, "ThresholdCount");

print(concat("Arrival count: ", numtostring(count), " of ", numtostring(threshold)));

if(count >= threshold) {
    // Threshold reached - open the queue
    closeoutput(queue, 0);
    setcolor(queue, "green");
    print("Queue1 OPENED - Threshold reached!");
    
    // Reset counter
    setlabelnum(current, "ArrivalCount", 0);
    
} else {
    // Keep queue closed
    closeoutput(queue, 1);
    setcolor(queue, "red");
}
```

---

## Scenario 8: Zone-Based Queue Control (Multiple Control Points)

### Setup:
- Multiple control points define zones
- Each zone controls different queues

### Control Point 1 (Zone A) - On Arrival
```c
// Entering Zone A
treenode transporter = parnode(1);

// Set transporter zone label
setlabelstr(transporter, "CurrentZone", "A");

// Control queues for Zone A
closeoutput(node("/QueueA1", model()), 0); // Open A1
closeoutput(node("/QueueA2", model()), 0); // Open A2
closeoutput(node("/QueueB1", model()), 1); // Close B1
closeoutput(node("/QueueB2", model()), 1); // Close B2

print("Transporter entered Zone A - A queues opened");
```

### Control Point 2 (Zone B) - On Arrival
```c
// Entering Zone B
treenode transporter = parnode(1);

// Set transporter zone label
setlabelstr(transporter, "CurrentZone", "B");

// Control queues for Zone B
closeoutput(node("/QueueA1", model()), 1); // Close A1
closeoutput(node("/QueueA2", model()), 1); // Close A2
closeoutput(node("/QueueB1", model()), 0); // Open B1
closeoutput(node("/QueueB2", model()), 0); // Open B2

print("Transporter entered Zone B - B queues opened");
```

---

## Scenario 9: Priority-Based Queue Control

### Control Point - On Arrival Trigger
```c
// Control queue access based on transporter priority

treenode transporter = parnode(1);
int priority = getlabelnum(transporter, "Priority");

treenode expressQueue = node("/ExpressQueue", model());
treenode standardQueue = node("/StandardQueue", model());

if(priority >= 5) {
    // High priority - open express queue
    closeoutput(expressQueue, 0);
    closeoutput(standardQueue, 1);
    
    setcolor(expressQueue, "green");
    setcolor(standardQueue, "red");
    
    print("HIGH PRIORITY: Express queue opened");
    
} else {
    // Normal priority - open standard queue
    closeoutput(expressQueue, 1);
    closeoutput(standardQueue, 0);
    
    setcolor(expressQueue, "red");
    setcolor(standardQueue, "green");
    
    print("NORMAL PRIORITY: Standard queue opened");
}
```

---

## Scenario 10: Batch Control (Open Queue When All Transporters Arrive)

### Control Point - On Reset Trigger
```c
// Initialize batch tracking
setlabelnum(current, "ArrivedCount", 0);
setlabelnum(current, "BatchSize", 3); // Wait for 3 transporters

treenode queue = node("/Queue1", model());
closeoutput(queue, 1); // Start with queue closed
```

### Control Point - On Arrival Trigger
```c
// Track batch arrivals

treenode transporter = parnode(1);
treenode queue = node("/Queue1", model());

// Increment arrival count
int arrived = getlabelnum(current, "ArrivedCount") + 1;
setlabelnum(current, "ArrivedCount", arrived);

int batchSize = getlabelnum(current, "BatchSize");

print(concat("Batch progress: ", numtostring(arrived), "/", numtostring(batchSize)));

if(arrived >= batchSize) {
    // Full batch arrived - open queue
    closeoutput(queue, 0);
    setcolor(queue, "green");
    print("BATCH COMPLETE - Queue opened!");
    
    // Reset for next batch
    setlabelnum(current, "ArrivedCount", 0);
    
    // Optional: Auto-close after short delay
    delayedmsg(current, 30, 1, queue); // Close after 30 seconds
}
```

### Control Point - On Message Trigger
```c
// Auto-close queue after batch processing

int messageCode = msgparam(1);
treenode queue = msgparam(2);

if(messageCode == 1) {
    closeoutput(queue, 1);
    setcolor(queue, "red");
    print("Queue auto-closed - Ready for next batch");
}
```

---

## Scenario 11: Sequential Queue Opening (Cascade Effect)

### Control Point - On Arrival Trigger
```c
// Open queues in sequence with delays

treenode transporter = parnode(1);

// Open first queue immediately
treenode queue1 = node("/Queue1", model());
closeoutput(queue1, 0);
setcolor(queue1, "green");
print("Queue1 opened immediately");

// Schedule Queue2 to open after 10 seconds
treenode queue2 = node("/Queue2", model());
delayedmsg(current, 10, 2, queue2);

// Schedule Queue3 to open after 20 seconds
treenode queue3 = node("/Queue3", model());
delayedmsg(current, 20, 3, queue3);

// Schedule Queue4 to open after 30 seconds
treenode queue4 = node("/Queue4", model());
delayedmsg(current, 30, 4, queue4);
```

### Control Point - On Message Trigger
```c
// Handle sequential queue openings

int queueNumber = msgparam(1);
treenode queue = msgparam(2);

// Open the scheduled queue
closeoutput(queue, 0);
setcolor(queue, "green");

print(concat("Queue", numtostring(queueNumber - 1), " opened at time: ", 
             numtostring(time())));
```

---

## Scenario 12: Dynamic Queue Control Using Global Table

### Global Table Setup
Create a table named "QueueControlTable" with columns:
- Row headers: Control point names
- Column 1: Queue to control (path)
- Column 2: Action (0=open, 1=close)

### Control Point - On Arrival Trigger
```c
// Read queue control instructions from global table

treenode transporter = parnode(1);
Table controlTable = Table("QueueControlTable");

// Find this control point in the table
string cpName = getname(current);
int rowNum = 0;

for(int i = 1; i < controlTable.numRows; i++) {
    if(strcmp(controlTable[i][0], cpName) == 0) {
        rowNum = i;
        break;
    }
}

if(rowNum > 0) {
    // Get queue path and action from table
    string queuePath = controlTable[rowNum][1];
    int action = controlTable[rowNum][2];
    
    treenode queue = node(queuePath, model());
    
    if(objectexists(queue)) {
        closeoutput(queue, action);
        
        if(action == 0) {
            setcolor(queue, "green");
            print(concat(queuePath, " OPENED by ", cpName));
        } else {
            setcolor(queue, "red");
            print(concat(queuePath, " CLOSED by ", cpName));
        }
    }
}
```

---

## Utility Functions

### Function: Open All Queues in a Group
```c
// User Command or Custom Function
void openAllQueues(string groupName) {
    Array queueGroup = group(groupName);
    
    for(int i = 1; i <= queueGroup.length; i++) {
        treenode queue = queueGroup[i];
        closeoutput(queue, 0);
        setcolor(queue, "green");
    }
    
    print(concat("All queues in group '", groupName, "' opened"));
}
```

### Function: Close All Queues in a Group
```c
// User Command or Custom Function
void closeAllQueues(string groupName) {
    Array queueGroup = group(groupName);
    
    for(int i = 1; i <= queueGroup.length; i++) {
        treenode queue = queueGroup[i];
        closeoutput(queue, 1);
        setcolor(queue, "red");
    }
    
    print(concat("All queues in group '", groupName, "' closed"));
}
```

### Function: Get Queue Status
```c
// Check if queue is open or closed
int isQueueOpen(treenode queue) {
    int status = getoutputstatus(queue);
    return (status == 0) ? 1 : 0; // Return 1 if open, 0 if closed
}
```

---

## Important FlexSim Functions Reference

### Queue Control Functions:
```c
// Close output (prevent items from leaving)
closeoutput(queue, 1);  // 1 = close
closeoutput(queue, 0);  // 0 = open

// Close input (prevent items from entering)
closeinput(queue, 1);   // 1 = close
closeinput(queue, 0);   // 0 = open

// Check status
int outputStatus = getoutputstatus(queue); // 0=open, 1=closed
int inputStatus = getinputstatus(queue);   // 0=open, 1=closed

// Stop output (release one item then stop)
stopoutput(queue);
```

### Control Point Trigger Parameters:
```c
// In Control Point triggers:
treenode transporter = parnode(1);  // The transporter at the control point
treenode item = parnode(2);         // Item being transported (if any)

// Transporter properties
int itemCount = content(transporter);
double speed = getspeed(transporter);
```

---

## Debugging Tips

### Visual Feedback
```c
// Add visual indicators for queue state
if(isQueueOpen) {
    setcolor(queue, "green");
    setlabelstr(queue, "DisplayLabel", "OPEN");
} else {
    setcolor(queue, "red");
    setlabelstr(queue, "DisplayLabel", "CLOSED");
}
```

### Console Logging
```c
// Detailed logging
print(concat("Time: ", numtostring(time())));
print(concat("Control Point: ", getname(current)));
print(concat("Transporter: ", getname(transporter)));
print(concat("Queue Status: ", (isOpen ? "OPEN" : "CLOSED")));
print("----------------------------------------");
```

### Dashboard Display
```c
// Update dashboard label
setlabelnum(model(), "Queue1Status", getoutputstatus(queue));
setlabelnum(model(), "LastControlTime", time());
setlabelstr(model(), "LastTransporter", getname(transporter));
```

---

## Common Pitfalls and Solutions

### Issue 1: Queue doesn't respond to control
**Solution**: Check port connections and ensure queue has proper output connections.

### Issue 2: Multiple control points conflict
**Solution**: Use flags/labels to track which control point should have priority.

### Issue 3: Queue opens but items don't flow
**Solution**: Check downstream capacity and ensure receiving objects are not full.

---

## Complete Example: Production Line with Transporter-Controlled Gates

This example shows a complete setup where an AGV controls different production queues based on its route.

### Model Setup:
1. AGV network with 3 control points (CP_A, CP_B, CP_C)
2. Three queues: QueueA, QueueB, QueueC
3. AGV transports items and controls which queue releases items

### CP_A - On Arrival
```c
treenode agv = parnode(1);
closeoutput(node("/QueueA", model()), 0); // Open A
closeoutput(node("/QueueB", model()), 1); // Close B
closeoutput(node("/QueueC", model()), 1); // Close C
setlabelstr(agv, "ActiveZone", "A");
```

### CP_B - On Arrival
```c
treenode agv = parnode(1);
closeoutput(node("/QueueA", model()), 1); // Close A
closeoutput(node("/QueueB", model()), 0); // Open B
closeoutput(node("/QueueC", model()), 1); // Close C
setlabelstr(agv, "ActiveZone", "B");
```

### CP_C - On Arrival
```c
treenode agv = parnode(1);
closeoutput(node("/QueueA", model()), 1); // Close A
closeoutput(node("/QueueB", model()), 1); // Close B
closeoutput(node("/QueueC", model()), 0); // Open C
setlabelstr(agv, "ActiveZone", "C");
```

This creates a production system where only the zone the AGV is currently servicing can release items.

---

## Summary

**Key Points:**
1. Use `closeoutput(queue, 1/0)` to control queue state
2. Control Point triggers: On Arrival, On Continue, On Message
3. Access transporter with `parnode(1)` in control point triggers
4. Use labels to track state and implement complex logic
5. Use `delayedmsg()` for time-based control
6. Implement visual feedback with `setcolor()` for easy debugging

**Best Practice:**
- Always initialize queue states in On Reset trigger
- Add logging for debugging
- Use visual indicators (colors, labels)
- Test with simple scenarios first
- Document which control points affect which queues
