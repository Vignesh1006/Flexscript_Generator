# FlexSim Simulation Code Examples - Complete Guide

## Table of Contents
1. Basic Manufacturing Line
2. Warehouse Operations
3. AGV/Transporter Systems
4. Process Flow with Decision Logic
5. Batching and Assembly Operations
6. Healthcare Patient Flow
7. Airport Baggage Handling
8. Restaurant/Service Operations
9. Advanced Logic and Custom Code
10. Statistical Analysis and Optimization

---

## 1. Basic Manufacturing Line

### Scenario: Simple Production Line
A basic manufacturing line with source → processor → queue → sink flow with setup times and breakdowns.

### Setup:
- Source: Generates products
- Processor1: Processing station (cycle time: 20s)
- Queue1: Buffer storage
- Sink: Final destination

### Code Examples:

#### Source - Arrival Pattern (On Creation Trigger)
```c
// Exponential inter-arrival time with mean of 30 seconds
return exponential(30, 1);
```

#### Source - Item Type Assignment
```c
// Randomly assign item types (70% Type 1, 30% Type 2)
if(duniform(1,100) <= 70)
    return 1; // Type 1
else
    return 2; // Type 2
```

#### Processor - Process Time (Process Time Trigger)
```c
// Get the item type
int itemType = getitemtype(item);

// Different process times based on item type
if(itemType == 1)
    return normal(20, 2, 1); // Mean 20s, StdDev 2s
else
    return normal(30, 3, 1); // Mean 30s, StdDev 3s
```

#### Processor - Setup Time (Setup Time Trigger)
```c
// Get previous and current item types
int prevType = getitemtype(previtem);
int currType = getitemtype(item);

// Setup required if different types
if(prevType != currType)
    return triangular(5, 10, 15, 1); // Setup: 5-10-15 seconds
else
    return 0; // No setup needed
```

#### Processor - Breakdown Schedule (MTBF/MTTR)
```c
// On Reset Trigger - Schedule first breakdown
double mtbf = 3600; // Mean Time Between Failures (1 hour)
double mttr = 300;  // Mean Time To Repair (5 minutes)

scheduledowntime(current, exponential(mtbf, 1));
```

```c
// On Breakdown Trigger
double mttr = 300;
return exponential(mttr, 1); // Return repair time
```

```c
// On Resume Trigger
double mtbf = 3600;
scheduledowntime(current, exponential(mtbf, 1)); // Schedule next breakdown
```

---

## 2. Warehouse Operations

### Scenario: Order Picking and Packing
Warehouse with multiple racks, operators picking items, and packing stations.

### Setup:
- Racks (3D objects)
- Operators (Task Executers)
- Packing Stations (Processors)
- Conveyor System

#### Queue/Rack - Pull Strategy (Custom Code)
```c
// Pull from rack based on FIFO or SKU priority
treenode rack = node("/Rack1", model());
treenode item = first(rack);

// Find highest priority item
treenode bestItem = NULL;
double highestPriority = -999999;

for(int i = 1; i <= content(rack); i++) {
    treenode checkItem = rank(rack, i);
    double priority = getlabelnum(checkItem, "Priority");
    
    if(priority > highestPriority) {
        highestPriority = priority;
        bestItem = checkItem;
    }
}

return bestItem;
```

#### Operator - Task Logic (On Message Trigger)
```c
// Custom picking task
if(msgparam(1) == 1) { // Pick task
    treenode item = msgparam(2);
    treenode fromLoc = msgparam(3);
    treenode toLoc = msgparam(4);
    
    // Travel to pick location
    traveltoloc(current, fromLoc);
    
    // Pick item (load time)
    delay(uniform(5, 10, 1));
    load(current, item);
    
    // Travel to drop location
    traveltoloc(current, toLoc);
    
    // Place item (unload time)
    delay(uniform(3, 7, 1));
    unload(current, item);
}
```

#### Rack - Dynamic Slotting Logic
```c
// On Entry Trigger - Assign to optimal slot
treenode item = parnode(1);
treenode rack = current;

// Find empty slot closest to dispatch point
double minDist = 999999;
int bestSlot = 1;

for(int i = 1; i <= capacity(rack); i++) {
    if(getvarnum(rank(rack, i), "state") == 0) { // Empty
        Vec3 slotPos = getloc(rank(rack, i));
        Vec3 dispatchPos = getloc(node("/DispatchPoint", model()));
        double dist = Vec3.distance(slotPos, dispatchPos);
        
        if(dist < minDist) {
            minDist = dist;
            bestSlot = i;
        }
    }
}

return bestSlot;
```

---

## 3. AGV/Transporter Systems

### Scenario: AGV Fleet Management
Multiple AGVs transporting materials between workstations with traffic control.

#### AGV Navigator - Control Point Logic
```c
// Control Point - Traffic Management
treenode cp = current;
int maxAllowed = 1; // Only 1 AGV at a time

// Count AGVs currently allocated to this control point
int agvCount = 0;
for(int i = 1; i <= content(cp); i++) {
    if(getvarnum(rank(cp, i), "allocated") == 1)
        agvCount++;
}

if(agvCount >= maxAllowed)
    return 0; // Block entry
else
    return 1; // Allow entry
```

#### AGV - Battery Management
```c
// On Travel Complete Trigger
double batteryLevel = getlabelnum(current, "BatteryLevel");
double distanceTraveled = msgparam(1);
double consumptionRate = 0.05; // 5% per 100 meters

batteryLevel -= (distanceTraveled / 100) * consumptionRate;
setlabelnum(current, "BatteryLevel", batteryLevel);

// If battery low, send to charging station
if(batteryLevel < 20) {
    treenode chargingStation = node("/ChargingStation1", model());
    traveltoloc(current, chargingStation);
    
    // Charging time
    delay(1800); // 30 minutes
    setlabelnum(current, "BatteryLevel", 100);
}
```

#### AGV - Task Assignment (Dispatch Logic)
```c
// Find nearest available AGV
treenode dispatcher = current;
treenode bestAGV = NULL;
double minDist = 999999;

// Get all AGVs
for(int i = 1; i <= content(group("AGVFleet")); i++) {
    treenode agv = rank(group("AGVFleet"), i);
    
    // Check if AGV is available
    if(getvarnum(agv, "state") == STATE_IDLE) {
        double dist = gettraveltime(agv, current);
        
        if(dist < minDist) {
            minDist = dist;
            bestAGV = agv;
        }
    }
}

if(bestAGV) {
    // Assign task
    settaskpriority(bestAGV, 1);
    // Add transport task logic here
}
```

---

## 4. Process Flow with Decision Logic

### Scenario: Quality Control with Rework
Products go through inspection; defective items are reworked or scrapped.

#### Processor - Quality Check (On Process Finish Trigger)
```c
// Quality inspection logic
double defectRate = 0.05; // 5% defect rate
treenode item = parnode(1);

if(duniform(1,100) <= defectRate * 100) {
    // Defective item
    setlabelnum(item, "Quality", 0);
    setcolor(item, "red");
    
    // Determine if reworkable
    if(duniform(1,100) <= 70) { // 70% can be reworked
        setlabelnum(item, "Rework", 1);
        
        // Send to rework station
        sendto(item, node("/ReworkStation", model()));
    } else {
        // Scrap item
        setlabelnum(item, "Scrap", 1);
        sendto(item, node("/ScrapBin", model()));
    }
} else {
    // Good quality
    setlabelnum(item, "Quality", 1);
    setcolor(item, "green");
    
    // Send to next station
    sendto(item, node("/PackingStation", model()));
}

return 0;
```

#### Decision Point - Routing Logic
```c
// Route items based on multiple criteria
treenode item = parnode(1);
int itemType = getitemtype(item);
int priority = getlabelnum(item, "Priority");
int quality = getlabelnum(item, "Quality");

// Complex routing decision
if(quality == 0) {
    if(getlabelnum(item, "Rework") == 1)
        return 1; // Port 1: Rework
    else
        return 2; // Port 2: Scrap
} else {
    if(priority == 1)
        return 3; // Port 3: Express packing
    else if(itemType == 1)
        return 4; // Port 4: Standard packing A
    else
        return 5; // Port 5: Standard packing B
}
```

---

## 5. Batching and Assembly Operations

### Scenario: Component Assembly
Multiple components are batched together and assembled into final products.

#### Combiner - Batch Assembly Logic
```c
// On Entry Trigger - Component arrival
treenode item = parnode(1);
treenode combiner = current;
int componentType = getitemtype(item);

// Track component arrivals
string labelName = concat("Component", numtostring(componentType), "Count");
int currentCount = getlabelnum(combiner, labelName);
setlabelnum(combiner, labelName, currentCount + 1);

// Check if all components are available
int comp1 = getlabelnum(combiner, "Component1Count");
int comp2 = getlabelnum(combiner, "Component2Count");
int comp3 = getlabelnum(combiner, "Component3Count");

// Assembly recipe: 2 of Type1, 1 of Type2, 3 of Type3
if(comp1 >= 2 && comp2 >= 1 && comp3 >= 3) {
    // Reset counts
    setlabelnum(combiner, "Component1Count", comp1 - 2);
    setlabelnum(combiner, "Component2Count", comp2 - 1);
    setlabelnum(combiner, "Component3Count", comp3 - 3);
    
    // Create assembled product
    treenode assembled = createinstance(node("/ItemBin/AssembledProduct", model()), combiner);
    setlabelnum(assembled, "AssemblyTime", time());
    
    return 1; // Release assembled product
}

return 0; // Wait for more components
```

#### Separator - Disassembly/Unpacking
```c
// On Entry Trigger - Unpack pallet into individual items
treenode pallet = parnode(1);
int itemCount = getlabelnum(pallet, "ItemCount");

// Create individual items
for(int i = 1; i < itemCount; i++) {
    treenode newItem = createcopy(pallet, current);
    setlabelnum(newItem, "ItemNumber", i);
    setlabelstr(newItem, "SourcePallet", getname(pallet));
}

// Original pallet becomes first item
setlabelnum(pallet, "ItemNumber", itemCount);
```

---

## 6. Healthcare Patient Flow

### Scenario: Emergency Department
Patients arrive, get triaged, wait, receive treatment, and depart.

#### Source - Patient Arrival (On Creation)
```c
// Patient arrival patterns by time of day
double currentTime = time();
double hourOfDay = fmod(currentTime / 3600, 24);

double arrivalRate;
if(hourOfDay >= 6 && hourOfDay < 12) {
    arrivalRate = 15; // Morning: 15 min average
} else if(hourOfDay >= 12 && hourOfDay < 18) {
    arrivalRate = 10; // Afternoon: 10 min average
} else if(hourOfDay >= 18 && hourOfDay < 24) {
    arrivalRate = 8;  // Evening: 8 min average (busy)
} else {
    arrivalRate = 20; // Night: 20 min average
}

return exponential(arrivalRate * 60, 1); // Convert to seconds
```

#### Patient - Triage Assignment
```c
// On Entry to Triage - Assign acuity level
treenode patient = parnode(1);

// ESI (Emergency Severity Index) 1-5
double rand = uniform(0, 1, 1);
int acuity;

if(rand < 0.05)
    acuity = 1; // Critical - 5%
else if(rand < 0.15)
    acuity = 2; // Emergent - 10%
else if(rand < 0.45)
    acuity = 3; // Urgent - 30%
else if(rand < 0.80)
    acuity = 4; // Less Urgent - 35%
else
    acuity = 5; // Non-Urgent - 20%

setlabelnum(patient, "Acuity", acuity);
setlabelnum(patient, "ArrivalTime", time());

// Color code patients
if(acuity == 1) setcolor(patient, "darkred");
else if(acuity == 2) setcolor(patient, "red");
else if(acuity == 3) setcolor(patient, "yellow");
else if(acuity == 4) setcolor(patient, "green");
else setcolor(patient, "blue");
```

#### Treatment Room - Process Time
```c
// Treatment time based on acuity
treenode patient = parnode(1);
int acuity = getlabelnum(patient, "Acuity");

double treatmentTime;
switch(acuity) {
    case 1: treatmentTime = triangular(30, 60, 120, 1); break;  // 30-60-120 min
    case 2: treatmentTime = triangular(20, 40, 90, 1); break;   // 20-40-90 min
    case 3: treatmentTime = triangular(15, 30, 60, 1); break;   // 15-30-60 min
    case 4: treatmentTime = triangular(10, 20, 40, 1); break;   // 10-20-40 min
    case 5: treatmentTime = triangular(5, 15, 30, 1); break;    // 5-15-30 min
}

return treatmentTime * 60; // Convert to seconds
```

#### Waiting Room - Priority Queuing
```c
// Pull from queue based on acuity (lowest number = highest priority)
treenode queue = current;
treenode selectedPatient = NULL;
int highestPriority = 999;

for(int i = 1; i <= content(queue); i++) {
    treenode patient = rank(queue, i);
    int acuity = getlabelnum(patient, "Acuity");
    double waitTime = time() - getlabelnum(patient, "ArrivalTime");
    
    // Increase priority if waiting too long
    int adjustedAcuity = acuity;
    if(waitTime > 3600 && acuity > 1) // After 1 hour
        adjustedAcuity--;
    
    if(adjustedAcuity < highestPriority) {
        highestPriority = adjustedAcuity;
        selectedPatient = patient;
    }
}

return selectedPatient;
```

---

## 7. Airport Baggage Handling

### Scenario: Baggage Sorting System
Bags arrive, get scanned, sorted to correct flight/gate, and loaded onto carts.

#### Conveyor - Bag Scanning
```c
// On Entry Trigger - Assign bag properties
treenode bag = parnode(1);

// Random flight assignment
int flightNumber = duniform(100, 150, 1);
setlabelnum(bag, "FlightNumber", flightNumber);

// Gate assignment (Gates 1-20)
int gate = ceil(flightNumber / 3.0) % 20 + 1;
setlabelnum(bag, "Gate", gate);

// Bag weight (for capacity planning)
double weight = normal(20, 5, 1); // Mean 20kg, StdDev 5kg
setlabelnum(bag, "Weight", max(5, weight));

// Departure time (next 2-8 hours)
double departureTime = time() + uniform(7200, 28800, 1);
setlabelnum(bag, "DepartureTime", departureTime);

// Scan result (99.9% successful)
if(duniform(1, 1000) <= 999)
    setlabelnum(bag, "Scanned", 1);
else
    setlabelnum(bag, "Scanned", 0); // Manual handling needed
```

#### Sorter - Diverter Logic
```c
// Divert bags to correct gate
treenode bag = parnode(1);
int gate = getlabelnum(bag, "Gate");
int scanned = getlabelnum(bag, "Scanned");

if(!scanned) {
    return 21; // Port 21: Manual handling area
}

// Time-based urgency routing
double departureTime = getlabelnum(bag, "DepartureTime");
double timeUntilDeparture = departureTime - time();

if(timeUntilDeparture < 1800) { // Less than 30 min
    setlabelnum(bag, "Priority", 1);
    return gate + 20; // Express lanes (ports 21-40)
} else {
    return gate; // Normal routing (ports 1-20)
}
```

#### Cart - Loading Logic
```c
// On Entry Trigger - Load bags onto cart
treenode cart = current;
treenode bag = parnode(1);

double currentWeight = getlabelnum(cart, "CurrentWeight");
double bagWeight = getlabelnum(bag, "Weight");
double maxCapacity = 500; // 500 kg max

currentWeight += bagWeight;
setlabelnum(cart, "CurrentWeight", currentWeight);

int bagCount = getlabelnum(cart, "BagCount") + 1;
setlabelnum(cart, "BagCount", bagCount);

// Check if cart is full
if(currentWeight >= maxCapacity || bagCount >= 30) {
    setlabelnum(cart, "Full", 1);
    
    // Dispatch cart to gate
    int gate = getlabelnum(bag, "Gate");
    treenode gateLocation = node(concat("/Gate", numtostring(gate)), model());
    
    // Create transport task
    createtask(cart, gateLocation, 0);
    
    // Reset cart after delivery
    setlabelnum(cart, "CurrentWeight", 0);
    setlabelnum(cart, "BagCount", 0);
    setlabelnum(cart, "Full", 0);
}
```

---

## 8. Restaurant/Service Operations

### Scenario: Quick Service Restaurant
Customers arrive, order, wait for food, eat, and leave.

#### Source - Customer Arrival
```c
// Customer arrival based on meal periods
double currentTime = time();
double hourOfDay = fmod(currentTime / 3600, 24);

double arrivalRate;
if(hourOfDay >= 11 && hourOfDay < 14) {
    arrivalRate = 2; // Lunch rush: 2 min between customers
} else if(hourOfDay >= 17 && hourOfDay < 20) {
    arrivalRate = 2.5; // Dinner rush: 2.5 min
} else if(hourOfDay >= 7 && hourOfDay < 10) {
    arrivalRate = 5; // Breakfast: 5 min
} else {
    arrivalRate = 15; // Off-peak: 15 min
}

return exponential(arrivalRate * 60, 1);
```

#### Customer - Order Assignment
```c
// On Creation - Assign order details
treenode customer = parnode(1);

// Party size
int partySize = discreteempirical(1, 0.6,  // 60% solo
                                   2, 0.25, // 25% pairs
                                   3, 0.10, // 10% groups of 3
                                   4, 0.05, 1); // 5% groups of 4+
setlabelnum(customer, "PartySize", partySize);

// Order complexity (items per person)
int itemsPerPerson = discreteempirical(1, 0.3,  // 30% just drink
                                       2, 0.5,  // 50% meal + drink
                                       3, 0.15, // 15% meal + side + drink
                                       4, 0.05, 1); // 5% large order
int totalItems = partySize * itemsPerPerson;
setlabelnum(customer, "TotalItems", totalItems);

// Special requests (10% of customers)
if(duniform(1,100) <= 10)
    setlabelnum(customer, "SpecialRequest", 1);
else
    setlabelnum(customer, "SpecialRequest", 0);
```

#### Order Station - Service Time
```c
// Ordering process time
treenode customer = parnode(1);
int totalItems = getlabelnum(customer, "TotalItems");
int specialRequest = getlabelnum(customer, "SpecialRequest");

// Base time: 20 seconds
// +10 seconds per item
// +30 seconds if special request
double serviceTime = 20 + (totalItems * 10) + (specialRequest * 30);

// Add variability
serviceTime = normal(serviceTime, serviceTime * 0.1, 1);

return max(10, serviceTime); // Minimum 10 seconds
```

#### Kitchen - Food Preparation
```c
// On Entry Trigger - Start cooking
treenode order = parnode(1);
int totalItems = getlabelnum(order, "TotalItems");

// Different prep times for different item types
double prepTime = 0;

for(int i = 1; i <= totalItems; i++) {
    int itemType = duniform(1, 5, 1);
    
    switch(itemType) {
        case 1: prepTime += triangular(60, 90, 120, 1); break;   // Burger
        case 2: prepTime += triangular(120, 180, 240, 1); break; // Pizza
        case 3: prepTime += triangular(30, 45, 60, 1); break;    // Salad
        case 4: prepTime += triangular(90, 120, 150, 1); break;  // Pasta
        case 5: prepTime += triangular(10, 20, 30, 1); break;    // Drinks
    }
}

// Multiple items can be prepared in parallel (reduce by 30%)
if(totalItems > 1)
    prepTime *= 0.7;

return prepTime;
```

#### Dining Table - Eating Duration
```c
// Customer eating and dining time
treenode customer = parnode(1);
int partySize = getlabelnum(customer, "PartySize");

// Base eating time: 15-20 minutes
double eatingTime = uniform(900, 1200, 1);

// Larger parties tend to stay longer
eatingTime *= (1 + (partySize - 1) * 0.15);

// Weekend/leisure dining (longer stays)
double hourOfDay = fmod(time() / 3600, 24);
if(hourOfDay >= 18 || hourOfDay < 2) // Evening/night
    eatingTime *= 1.3;

return eatingTime;
```

---

## 9. Advanced Logic and Custom Code

### Scenario: Dynamic Resource Allocation

#### Global Table - Resource Management
```c
// On Reset - Initialize resource tracking table
Table resourceTable = Table("ResourceAvailability");

// Clear existing data
resourceTable.setSize(10, 4); // 10 resources, 4 columns

// Headers
resourceTable[0][1] = "ResourceName";
resourceTable[0][2] = "Available";
resourceTable[0][3] = "Busy";
resourceTable[0][4] = "Utilization";

// Initialize resources
for(int i = 1; i <= 9; i++) {
    resourceTable[i][1] = concat("Resource", numtostring(i));
    resourceTable[i][2] = 1; // Available
    resourceTable[i][3] = 0; // Not busy
    resourceTable[i][4] = 0; // 0% utilization
}
```

#### Custom Resource Allocation Function
```c
// User Command - Find best available resource
treenode findBestResource(int taskType, double priority) {
    Table resourceTable = Table("ResourceAvailability");
    treenode bestResource = NULL;
    double lowestUtilization = 999999;
    
    // Search through resources
    for(int i = 1; i < resourceTable.numRows; i++) {
        int available = resourceTable[i][2];
        double utilization = resourceTable[i][4];
        
        // Check skill match
        treenode resource = node(resourceTable[i][1], model());
        int skillLevel = getlabelnum(resource, concat("Skill", numtostring(taskType)));
        
        if(available && skillLevel >= priority) {
            if(utilization < lowestUtilization) {
                lowestUtilization = utilization;
                bestResource = resource;
            }
        }
    }
    
    // Mark resource as busy
    if(bestResource) {
        for(int i = 1; i < resourceTable.numRows; i++) {
            if(strcmp(resourceTable[i][1], getname(bestResource)) == 0) {
                resourceTable[i][2] = 0; // Not available
                resourceTable[i][3] = 1; // Busy
                break;
            }
        }
    }
    
    return bestResource;
}
```

#### Performance Tracking
```c
// On Exit Trigger - Track KPIs
treenode item = parnode(1);
double arrivalTime = getlabelnum(item, "ArrivalTime");
double exitTime = time();
double cycleTime = exitTime - arrivalTime;

// Update statistics
double avgCycleTime = getvarnum(model(), "AvgCycleTime");
double totalItems = getvarnum(model(), "TotalItems");

// Rolling average
avgCycleTime = ((avgCycleTime * totalItems) + cycleTime) / (totalItems + 1);
setvarnum(model(), "AvgCycleTime", avgCycleTime);
setvarnum(model(), "TotalItems", totalItems + 1);

// Track by product type
int productType = getitemtype(item);
string statName = concat("CycleTime_Type", numtostring(productType));
double typeAvg = getvarnum(model(), statName);
double typeCount = getvarnum(model(), concat("Count_Type", numtostring(productType)));

typeAvg = ((typeAvg * typeCount) + cycleTime) / (typeCount + 1);
setvarnum(model(), statName, typeAvg);
setvarnum(model(), concat("Count_Type", numtostring(productType)), typeCount + 1);

// Alert if cycle time exceeds threshold
if(cycleTime > 3600) { // 1 hour threshold
    msg("Performance Alert", concat("Item exceeded cycle time threshold: ", 
        numtostring(cycleTime/60), " minutes"));
}
```

---

## 10. Statistical Analysis and Optimization

### Scenario: Experimenter and Dashboard Setup

#### Experimenter Variables
```c
// Performance Variables to Track
// In Experimenter, add these as Performance Measures:

// 1. Throughput
double throughput = content(node("/Sink1", model())) / (time() / 3600);
// Returns: items per hour

// 2. Average WIP (Work in Progress)
double avgWIP = getstat(node("/Model"), "WIP Content", STAT_AVERAGE);

// 3. Average Cycle Time
double avgCycleTime = getstat(node("/Sink1"), "staytime", STAT_AVERAGE) / 60;
// Returns: minutes

// 4. Resource Utilization
double utilization = getstat(node("/Processor1"), "State Profile", STAT_PROCESSING) / 
                     time() * 100;
// Returns: percentage

// 5. Queue Time
double avgQueueTime = getstat(node("/Queue1"), "staytime", STAT_AVERAGE) / 60;
```

#### Dashboard - Real-time Metrics (On Dashboard Update)
```c
// Update dashboard every 60 seconds
double updateInterval = 60;

// Current throughput rate
int outputCount = content(node("/Sink1", model()));
double currentTime = time() / 3600; // hours
double throughputRate = outputCount / currentTime;
setlabelnum(model(), "DashboardThroughput", throughputRate);

// Current WIP level
int wipCount = 0;
Array processors = Group("AllProcessors");
for(int i = 1; i <= processors.length; i++) {
    wipCount += content(processors[i]);
}
setlabelnum(model(), "DashboardWIP", wipCount);

// Bottleneck identification
double maxUtil = 0;
treenode bottleneck = NULL;
for(int i = 1; i <= processors.length; i++) {
    double util = getstat(processors[i], "State Profile", STAT_PROCESSING) / time();
    if(util > maxUtil) {
        maxUtil = util;
        bottleneck = processors[i];
    }
}
setlabelstr(model(), "Bottleneck", getname(bottleneck));
setlabelnum(model(), "BottleneckUtil", maxUtil * 100);

// Schedule next update
schedulefunction(current, "updateDashboard", updateInterval);
```

#### Optimization - Scenario Comparison
```c
// On Reset - Set scenario parameters
int scenarioNumber = getvarnum(model(), "ScenarioNumber");

switch(scenarioNumber) {
    case 1: // Baseline
        setprocesstime(node("/Processor1", model()), 20);
        setcapacity(node("/Queue1", model()), 10);
        setmtbf(node("/Processor1", model()), 3600);
        break;
        
    case 2: // Faster processing
        setprocesstime(node("/Processor1", model()), 15);
        setcapacity(node("/Queue1", model()), 10);
        setmtbf(node("/Processor1", model()), 3600);
        break;
        
    case 3: // Larger buffer
        setprocesstime(node("/Processor1", model()), 20);
        setcapacity(node("/Queue1", model()), 20);
        setmtbf(node("/Processor1", model()), 3600);
        break;
        
    case 4: // Better reliability
        setprocesstime(node("/Processor1", model()), 20);
        setcapacity(node("/Queue1", model()), 10);
        setmtbf(node("/Processor1", model()), 7200); // Double MTBF
        break;
        
    case 5: // Combined improvements
        setprocesstime(node("/Processor1", model()), 15);
        setcapacity(node("/Queue1", model()), 15);
        setmtbf(node("/Processor1", model()), 5400);
        break;
}
```

---

## Common Utility Functions

### Time Conversion
```c
// Convert simulation time to readable format
string formatTime(double seconds) {
    int hours = floor(seconds / 3600);
    int minutes = floor((seconds - hours * 3600) / 60);
    int secs = floor(seconds - hours * 3600 - minutes * 60);
    
    return concat(numtostring(hours), ":", 
                  numtostring(minutes), ":", 
                  numtostring(secs));
}
```

### Distance Calculation
```c
// Calculate distance between two objects
double calculateDistance(treenode obj1, treenode obj2) {
    Vec3 pos1 = getloc(obj1);
    Vec3 pos2 = getloc(obj2);
    
    double dx = pos1.x - pos2.x;
    double dy = pos1.y - pos2.y;
    double dz = pos1.z - pos2.z;
    
    return sqrt(dx*dx + dy*dy + dz*dz);
}
```

### Random Selection with Weights
```c
// Select item based on weighted probabilities
int weightedRandom(Array weights) {
    double total = 0;
    for(int i = 1; i <= weights.length; i++) {
        total += weights[i];
    }
    
    double rand = uniform(0, total, 1);
    double cumulative = 0;
    
    for(int i = 1; i <= weights.length; i++) {
        cumulative += weights[i];
        if(rand <= cumulative)
            return i;
    }
    
    return weights.length;
}
```

---

## Best Practices

### 1. Naming Conventions
- Use descriptive names: `processingTime` not `pt`
- Use camelCase for variables: `itemType`, `cycleTime`
- Use consistent prefixes: `avg`, `max`, `min`, `total`

### 2. Performance Optimization
- Minimize searches through large arrays
- Use labels efficiently (don't create unnecessary labels)
- Cache frequently accessed values
- Use groups for object collections

### 3. Debugging
```c
// Print debugging information
print(concat("Current time: ", numtostring(time())));
print(concat("Item type: ", numtostring(getitemtype(item))));
print(concat("Queue content: ", numtostring(content(queue))));

// Conditional breakpoints
if(getlabelnum(item, "Priority") == 1) {
    stop(); // Pause simulation
}
```

### 4. Error Handling
```c
// Check for null objects
if(objectexists(targetNode)) {
    // Safe to proceed
} else {
    msg("Error", "Target node does not exist");
    return 0;
}

// Validate parameters
if(processTime < 0) {
    msg("Warning", "Negative process time, using default");
    processTime = 10;
}
```

---

## Additional Resources

### Common Triggers
- **On Reset**: Initialize variables, clear statistics
- **On Entry**: Item enters object
- **On Exit**: Item leaves object
- **On Creation**: Source creates new item
- **On Process Finish**: Processing completes
- **On Message**: Receives message from sendmessage()

### Key Functions
- `time()` - Current simulation time
- `content(obj)` - Number of items in object
- `getlabelnum(obj, "name")` - Get numeric label
- `setlabelnum(obj, "name", value)` - Set numeric label
- `getitemtype(item)` - Get item type number
- `sendto(item, destination)` - Route item to destination
- `createinstance(obj, location)` - Create new item

### Random Distributions
- `exponential(mean, stream)`
- `normal(mean, stddev, stream)`
- `uniform(min, max, stream)`
- `triangular(min, mode, max, stream)`
- `weibull(alpha, beta, stream)`
- `gamma(alpha, beta, stream)`

---

This guide covers the most common FlexSim scenarios. Each code block can be directly implemented in the appropriate trigger or function within your FlexSim model.
