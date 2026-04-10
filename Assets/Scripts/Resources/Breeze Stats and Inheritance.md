# Racing Stats

### General

* furlong \[Distance\]  
  * 5f  
  * 6f  
  * 7f  
  * 1m  
  * 1m1f  
  * 1m2f  
  * 1m3f  
  * 1m4f  
  * 1m5f  
  * 1m6f  
  * 1m7f  
  * 2m  
* surface  
  * Dirt  
  * Turf  
  * Synthetic  
* going  
  * Good  
  * Firm  
  * Fast  
  * Slow  
  * Muddy  
* preferredSurface  
* preferredGoing  
* potential  
* realizedPotential  
* fitness  
* reputation  
* condition \[Form\]  
  * Possibility to decrease each race  
  * Never increases  
  * Depends on deteriorationPotential  
* conditionPercent  
  * Regenerates over time, only decreases at rollover  
  * Inactive horses lose condition as well  
  * Only increases by racing

### Racing

* spd \[Speed\]  
* stamina  
* acceleration  
* start  
* strength  
* intelligence  
* movement  
* tenacity  
* enthusiasm  
* confidence  
* battlingQualities  
* cruisingBurst  
* extraSpeedRating  
* finishApplication  
* consistency  
* distanceAdaptability  
* goingSurfaceAdaptability  
* quirks

### Formula

* stats  
* actualCondition  
* actualFitness  
* performance  
* actualPerformance  
* maximumPerformance  
* actualBattlingQualities  
* actualCruisingBurst  
* actualExtraSpeedRating  
* actualFinishApplication

# Racing Formulas

* calculatePerformance (JavaScript)  
  * void method / function  
  * no parameters (assuming class-like structure)  
  * performance value used to determine the winner in a race is actually determined by actualPerformance  
    * the actualPerformance values are taken and a random number is picked between all values (not completely random as different horses will have different numbers of “tickets” based on their stats)  
  * complex system avoided by using a bonus system where high levels of certain stats contribute a bonus to actualPerformance

| function calculatePerformance(n) {  console.log("----------------");  stats \= spd \+ stamina \+ acceleration \+ start \+ strength \+ intelligence \+ movement;  actualCondition \= Math.round(condition \* (conditionPercent/100));  actualFitness \= Math.round(fitness \* (actualCondition / 100));  performance \= Math.round((reputation \* (actualFitness/100)) \+ stats);  actualPerformance \= Math.round(performance \* (realizedPotential/100));  maximumPerformance \= Math.round(performance \* (potential/100));  actualBattlingQualities \= Math.round(battlingQualities \* ((intelligence \+ strength \+ enthusiasm)/100));  actualCruisingBurst \= Math.round(cruisingBurst \* ((movement \+ spd \+ acceleration)/50));  actualExtraSpeedRating \= Math.round(extraSpeedRating \* ((start \+ movement \+ acceleration)/100));  actualFinishApplication \= Math.round(finishApplication \* ((spd \+ confidence \+ tenacity)/100));  //POTENTIAL  if (realizedPotential \>= 90) {    actualPerformance \= Math.round(actualPerformance\*1.5);    console.log("High Potential bonus");  }  //FINISH APPLICATION  if (actualFinishApplication \>= 100) {    actualPerformance \= Math.round(actualPerformance\*1.5);    console.log("Finish Application bonus");  }  //EXTRA SPEED RATING  if (actualExtraSpeedRating \>= 100) {    actualPerformance \= Math.round(actualPerformance\*1.5);    console.log("Extra Speed Rating bonus");  }  //CRUISING BURST  if (actualCruisingBurst \>= 90) {    actualPerformance \= Math.round(actualPerformance\*1.3);    console.log("Cruising Burst bonus");  }  //BATTLING QUALITIES  if (actualBattlingQualities \>= 80) {    actualPerformance \= Math.round(actualPerformance\*1.1);    console.log("Battling Qualities bonus");  }  //ACCELERATION  if (acceleration \>= 15) {    actualPerformance \= Math.round(actualPerformance\*1.05);    console.log("Acceleration bonus");  }  //START  if (start \>= 15) {    actualPerformance \= Math.round(actualPerformance\*1.5);    console.log("Start bonus");  }  //SURFACE  if (surface \== preferredSurface) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Horse loved the ground/surface");  } else {    if ((randomNumber(0, 100)) \>= goingSurfaceAdaptability) {      actualPerformance \= Math.round(actualPerformance \* (goingSurfaceAdaptability/100));      console.log("Surface not suitable");    }  }  //GOING  if (going \== preferredGoing) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Horse loved the going");  } else {    if ((randomNumber(0, 100)) \>= goingSurfaceAdaptability) {      actualPerformance \= Math.round(actualPerformance \* (goingSurfaceAdaptability/100));      console.log("Going not suitable");    }  }  //SPEED \- DISTANCE ADAPTABILITY  if (spd \>= 15 && furlong \<= 6) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Sprinter");  } else if (((spd \> 5 && spd \< 15) && (furlong \>= 7 && furlong \<= 9))) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Miler");  } else if ((spd \<= 5 && furlong \>= 10)) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Stayer");  } else {    if ((randomNumber(0, 100)) \>= distanceAdaptability) {      actualPerformance \= Math.round(actualPerformance \* (distanceAdaptability/100));      console.log("Distance not suitable");    }  }  //STAMINA \- DISTANCE ADAPTABILITY  if (stamina \<= 5 && furlong \<= 6) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Sprinter");  } else if (((stamina \> 5 && stamina \< 15) && (furlong \>= 7 && furlong \<= 9))) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Miler");  } else if ((stamina \>= 15 && furlong \>= 10)) {    actualPerformance \= Math.round(actualPerformance\*1.25);    console.log("Stayer");  } else {    if ((randomNumber(0, 100)) \>= distanceAdaptability) {      actualPerformance \= Math.round(actualPerformance \* (distanceAdaptability/100));      console.log("Distance not suitable");    }  }  //ENTHUSIASM  if (enthusiasm \<= 25) {    actualPerformance \= actualPerformance;    console.log("Disposition: Laid Back (can be lazy, but not affected by crowd noise)");  } else if (enthusiasm \< 75) {    actualPerformance \= actualPerformance;    console.log("Disposition: Normal");  } else if ((enthusiasm \<= 100 && (randomNumber(0, 100)) \<= enthusiasm)) {    actualPerformance \= Math.round(actualPerformance \* ((100 \- enthusiasm)/100));    console.log("Disposition: Excitable (likely to sweat up badly)");  }  //QUIRKS  if ((randomNumber(0, 100)) \<= quirks) {    actualPerformance \= Math.round(actualPerformance \* ((100 \- quirks)/100));    console.log("Did not want to leave the gate");  } else {    actualPerformance \= actualPerformance;  }  //CONSISTENCY  if (randomNumber(0, randomNumber(50, 100)) \>= consistency) {    actualPerformance \= Math.round(actualPerformance \* (randomNumber(randomNumber(25, 50), randomNumber(150, 200))/100));    console.log("Displayed inconsistency");  } else {    actualPerformance \= actualPerformance;  }  console.log("----------------");} |
| :---- |

# Horse Stats

* name  
* sire  
* sireMane  
* sireTail  
* sireNose  
* dam  
* damCoat  
* damCoatRarity  
* damModifier  
* damMarking  
* damMane  
* damTail  
* damNose  
* gender  
* coat  
* coatRarity  
* anomaly  
* mane  
* tail  
* nose

# Breeding Stats

* Progeny Potential  
  * A number from 0 to 2  
    * Special Case of 0 → unable to produce offspring OR if this is to difficult to implement, only yields offspring with worse stats  
    * Lower Numbers → less variety but offspring will have stats closer to the parent  
    * Higher Numbers → greater range so offspring can have stats close to the parent or very far from the parent (a lot lower or a lot higher)  
  * Depicts stat range for inheritance

# Breeding Formulas

* randomGender (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determine gender randomly (as in real-life)

|   // set random gender  public void randomGender() {    max \= 1;    min \= 0;    result \= (int) (Math.floor(Math.random() \* (max \- min \+ 1) \+ min));    gender \= genderOptions\[result\];  } |
| :---- |

* randomMane (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determines which random mane style to set to  
    * random number **result** calculated between 1 and 100  
    * \[5% chance\] **result** ≤ 5, mane is set to ★★★ Long Curly  
    * \[5% chance\] **result** ≤ 10, mane is set to ★★★ Long Straight  
    * \[10% chance\] **result** ≤ 20, mane is set to ★★ Cropped  
    * \[10% chance\] **result** ≤ 30, mane is set to ★★ Short  
    * \[70% chance\] **result** ≤ 100, mane is set to ★ Medium  
* randomTail (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determines which random tail style to set to  
    * random number **result** calculated between 1 and 100  
    * \[5% chance\] **result** ≤ 5, tail is set to ★★★ Long Curly  
    * \[5% chance\] **result** ≤ 10, tail is set to ★★★ Cropped  
    * \[10% chance\] **result** ≤ 20, tail is set to ★★ Long Straight  
    * \[10% chance\] **result** ≤ 30, tail is set to ★★ Short  
    * \[70% chance\] **result** ≤ 100, tail is set to ★ Medium  
* randomNose (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determines which random roman nose style to set to  
    * random number **result** calculated between 1 and 100  
    * \[5% chance\] **result** ≤ 5, nose is set to ★★★ Extreme  
    * \[15% chance\] **result** ≤ 20, nose is set to ★★ Subtle  
    * \[80% chance\] **result** ≤ 100, nose is set to ★ Evident  
* randomCoat (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determines which random coat to set to  
    * random number **result** calculated between 1 and 100  
    * \[5% chance\] **result** ≤ 5, coat is set to a random ★★★ coat  
* determineCoat (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determines if foal’s coat is hereditary or random, depends on dam  
    * first, sets a random coat to the foal  
    * random number **result** calculated between 0 and 1  
    * if the dam’s coat rarity is the same as the foal’s random coat rarity  
      * \[50% chance\] **result** is 0, override original random coat with the dam’s coat  
      * \[50% chance\] **result** is 1

  // determine if coat is hereditary or random  
  public void determineCoat() {  
    // sets a coat and a rarity  
    randomCoat();  
    max \= 1;  
    min \= 0;  
    result \= (int) (Math.floor(Math.random() \* (max \- min \+ 1\) \+ min));  
    if (damCoatRarity.equals(coatRarity)) {  
      switch (result) {  
        case 0:  
          // overrides original random coat  
          coat \= damCoat;  
          break;  
      }  
    }  
  }

* determineModifier (Java)  
  * public void method / function  
  * no parameters (assuming class-like structure)  
  * determines if foal’s modifiers are hereditary or random

# Cosmetics

* Skins are based on the breeding system  
  * For horses not bred but created,   
* Mane/tail styles  
* Varied heights  
  * For simplicity, maybe a scaling numerical from 0.75 to 1.25, for example

### Gender Options

\[50% Chance\]

* 0: Stallion / Colt  
* 1: Mare / Filly

### Mane Options

★ Medium  
★★ Cropped  
★★ Short  
★★★ Long Curly  
★★★ Long Straight

### Tail Options

★ Medium  
★★ Long Straight  
★★ Short  
★★★ Long Curly  
★★★ Cropped

### Nose Options

★ Evident  
★★ Subtle  
★★★ Extreme

### Coat Options

★ Bay  
★ Dark Bay  
★ Chestnut  
★ Gray  
★★ Liver Chestnut  
★★ Seal Brown  
★★ Wild Bay  
★★ Buckskin  
★★ Palomino  
★★ Smoky Black  
★★★ Black  
★★★ Smoky Cream  
★★★ Cremello  
★★★ Perlino  
★★★ Wild Buckskin

### Modifier Options

★ None  
★★ Gray  
★★ Flaxen  
★★★ Roan  
★★★ Sooty

### Modifier Anomaly Options

★★★ Non-Dun 1

### Dilution Options

★ None  
★★ Cream  
★★★ Silver  
★★★ Dun  
★★★ Pangaré  
★★★ Champagne  
★★★ Pearl  
★★★ Double Cream

### Dilution Anomaly Options

★★★ Mushroom  
★★★ Snowdrop  
★★★ Sunshine

### White Marking Options

★ None  
★★ Sabino  
★★ Splash  
★★ Dominant White  
★★★ Overo  
★★★ Tobiano  
★★★ Rabicano

### White Marking Anomaly Options

★★★ Leopard Complex

### Anomaly Options

★ Apron Face (sabino, splash, dominant white, or overo only)  
★ Bald Face (sabino, splash, dominant white, or overo only)  
★ Bonnet Face (splash, overo, or tobiano only)  
★ Medicine Hat (tobiano AND splash or overo only)  
★ War Bonnet (tobiano AND splash or overo only)  
★ Mealy Muzzle (pangaré only)  
★★ Ink Spots (tobiano only)  
★★ Reversed Markings (sabino, splash, dominant white, overo, or tobiano only)  
★★ Ermine Spots  
★★ Belton Pattern  
★★ Mismarks (leopard complex or gray only)  
★★ Corn Spots (roan only)  
★★★ Birdcatcher Spots  
★★★ Bend-Or Spots  
★★★ Chubari Spots (gray only)  
★★★ Bloody Shoulder (gray only)  
★★★ Gulastra Plume (sabino only)  
★★★ Badger Face (sabino, splash, or dominant white only)  
★★★ Reverse Badger Face (sabino, splash, or dominant white only)  
★★★ Chimera  
★★★ Somatic Mutation  
★★★ Brindle  
★★★ Reverse Brindle  
★★★ Reverse Dapples  
★★★ Lacing  
★★★ Varnish Roan (leopard complex only)  
★★★ Bider Marks (dun only)  
★★★ Manchado

# Stretch Goals

* Álogo Spirit of the \_\_\_\_\_ (chinese zodiac 1-12 randomized) provides stat boosts but also can be an indicator of their personality, goes from I to III with breeding (if you breed two horses ONLY with the same level the foal will be the next level)  
* Possibly the same with temperament and (race) position but introducing more rarity

* Temperament: no effect on stats (50% chance)  
* Spirit: slight boost on stats, only between SAME LEVEL parents (25% chance)  
* Position: boosts stats (10% chance)

* When breeding two level IIIs of any of these, slight chance to unlock new ones that cannot be leveled (maximum)  
* Temperament: bombproof (5% chance), benevolent (1% chance) 

# Performance & Trait Markers

* The "Speed Gene" (MSTN): Used in Thoroughbreds to determine if a horse is better suited for sprints or staying distances.  
* The Gait Gene (DMRT3): Often called the "Gait Keeper," this determines if a horse has the ability to pace or perform ambling gaits.  
* Temperament (DRD4): Emerging tests like those from Animal Genetics look for markers related to curiosity and vigilance.  
* Height (LCORL): A genetic marker that can predict a foal's potential mature height with significant accuracy.