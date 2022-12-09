# Overview
This plugin does not introduce any functionality by itself, but rather provides a framework for other plugins to manipulate the status list that appears in the game UI on the right.
# Developer API
``` csharp
/* 
Sets a status message for a player,
this message will persist until manually cleared.
*/
void SetStatus(
  BasePlayer basePlayer,
  string id,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId
);

/* 
Clears a status for a player.
*/
void ClearStatus(
  BasePlayer basePlayer,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId
);

/* 
Performs a Set and Clear and forces the UI to
update the status list. Useful if you for instance
want to increment a counter on a status message
or change the color.
*/
void UpdateStatus(
  BasePlayer basePlayer,
  string id,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId
);

/* 
Performs a Set and then automatically performs
a Clear after a set number of seconds. By default,
this value is 4 seconds.
*/
void ShowStatus(
  BasePlayer basePlayer,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId,
  float seconds? // defaults to 4 seconds if left out
);

/* 
Returns a list of status ids that a player currently has.
*/
List<string> GetStatusList(
  BasePlayer basePlayer
);

/* 
Returns true if the player has the specified status.
Status ids are case sensitive.
*/
bool HasStatus(
  BasePlayer basePlayer,
  string id
);

/* 
Creates a global status that will automatically be
applied to players that satisfy the specified condition.
*/
void CreateStatus(
string id,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId, 
  Func<BasePlayer, bool> condition
);

/* 
Creates a global status that contains a dynamic
value (such as a counter or timer) evaluated by
the dynamicValue property that will be
automatically applied to players that satisfy the
specified condition.
*/
void CreateDynamicStatus(
string id,
  string text, 
  string color, 
  string imageLibaryIconId, 
  Func<BasePlayer, bool> condition, 
  Func<BasePlayer, string> dynamicValue
);
```

# Code Examples
#### Example 1
Welcome status that will be displayed players who have been online for less than 60 seconds.

``` csharp
            Func<BasePlayer, bool> condition = (basePlayer) => 
            { 
                  return basePlayer.secondsConnected < 60; 
            };
            CustomStatusFramework.Call("CreateStatus", 
            "newplayer"
            "New Player", 
            "Welcome!", 
            "0.3 0.6 0.9 1", 
            "img_id_here", 
            condition);
```

#### Example 2
Displays a status for any players wearing both a santa hat and beard.

``` csharp
            Func<BasePlayer, bool> condition = (basePlayer) =>
            {
                var itemsWorn = basePlayer.inventory.containerWear;
                return itemsWorn.itemList.Any(item => item.info.shortname == "santahat") 
                && itemsWorn.itemList.Any(item => item.info.shortname == "santabeard");
            };
            CustomStatusFramework.Call("CreateStatus", 
            "santa", 
            "Santa", 
            "Ho ho ho!", 
            "0.9 0.3 0.3 1", 
            "img_id_here", 
            condition);
```