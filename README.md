Still WIP, do not review.

# Developer API
``` csharp
void SetStatus(
  BasePlayer basePlayer,
  string id,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId
);

void ClearStatus(
  BasePlayer basePlayer,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId
);

void UpdateStatus(
  BasePlayer basePlayer,
  string id,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId
);

void ShowStatus(
  BasePlayer basePlayer,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId,
  float seconds? // defaults to 4 seconds if left out
);

List<string> GetStatusList(
  BasePlayer basePlayer
);

bool HasStatus(
  BasePlayer basePlayer,
  string id
);

void CreateStatus(
string id,
  string text, 
  string subText, 
  string color, 
  string imageLibraryIconId, 
  Func<BasePlayer, bool> condition
);

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