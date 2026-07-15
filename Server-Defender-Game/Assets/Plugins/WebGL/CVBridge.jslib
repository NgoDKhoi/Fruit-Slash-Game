mergeInto(LibraryManager.library, {
    GetHandDataStringJS: function () {
        var returnStr = "";
        
        // window.GetHandDataString is defined in index.html template
        if (typeof window.GetHandDataString === 'function') {
            returnStr = window.GetHandDataString();
        }
        
        // Convert to a buffer that Unity can read (WASM Heap allocation)
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        
        return buffer;
    },

    SetCameraVisibleJS: function (isVisible) {
        if (typeof window.SetCameraVisible === 'function') {
            // isVisible is passed as an integer (1/0) from C# bool
            window.SetCameraVisible(isVisible === 1 || isVisible === true);
        }
    }
});
