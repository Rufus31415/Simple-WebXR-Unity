/**
 * Helpers for this example
 */

mergeInto(LibraryManager.library, {
  /****************************************************************************/
  // Call javascript prompt function
  Prompt: function (message, defaultValue) {
    var answer = prompt(Pointer_stringify(message), Pointer_stringify(defaultValue));
    if(!answer) answer = Pointer_stringify(defaultValue);
    var bufferSize = lengthBytesUTF8(answer) + 1;
    var bufferAnswer = _malloc(bufferSize);
    stringToUTF8(answer, bufferAnswer, bufferSize);
    return bufferAnswer;
  }
});