mergeInto(LibraryManager.library, {
    DownloadFile: function (fileNamePtr, byteArray, byteArraySize) {
        var fileName = UTF8ToString(fileNamePtr);

        // Create a Uint8Array from the Unity byte array
        var bytes = new Uint8Array(byteArraySize);
        for (var i = 0; i < byteArraySize; i++) {
            bytes[i] = HEAPU8[byteArray + i];
        }

        // Create a Blob from the byte array
        var blob = new Blob([bytes], { type: 'image/png' });

        // Create a temporary URL for the blob
        var url = URL.createObjectURL(blob);

        // Create a temporary anchor element and trigger download
        var link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();

        // Clean up
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
});
