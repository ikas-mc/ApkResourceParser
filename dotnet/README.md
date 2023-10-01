# apk resource parser for .net

port of https://github.com/google/android-arscblamer

# sample

### AndroidManifest.xml

``` c#
    var file = File.ReadAllBytes("AndroidManifest.xml");
    var resourceFile = new ResourceFile(new ByteBuffer(file));
    IList<Chunk> chunks = resourceFile.getChunks();
```

### resources.arsc

``` c#
    var file = File.ReadAllBytes("resources.arsc");
    var resourceFile = new ResourceFile(new ByteBuffer(file));
    IList<Chunk> chunks = resourceFile.getChunks();
    var resourceTableChunk = (ResourceTableChunk)chunks[0];
```

### icon

``` c#
    //1. icon ref id form AndroidManifest.xml
    var iconRefId =...;

    //2. resourceTableChunk from resources.arsc
    var resourceTableChunk =...;

    //3 .icon path ref list
    var id = ResourceIdentifier.create(iconRefId);
    var packageChunk = resourceTableChunk.getPackage(id.packageId);
    var typeChunks = packageChunk.getTypeChunks(id.typeId);
    var icons = typeChunks.Select(x =>
    {
        x.getEntries().TryGetValue(id.entryId, out var icon);
        return icon;
    }).Where(x => null != x).ToList();

    //4 .icon path list
    var stringPoolChunk = resourceTableChunk.getStringPool();
    var iconPaths = icons.Select(icon =>
    {
        var index = icon.value().data();
        var path = stringPoolChunk.getString(index);
        return path;
    }).ToList();
```

