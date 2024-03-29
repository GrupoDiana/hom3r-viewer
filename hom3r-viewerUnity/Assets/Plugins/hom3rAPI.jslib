mergeInto(LibraryManager.library, {

  Hello: function () {
    window.alert("Hello, world!");
  },

  HelloString: function (str) {
    //window.alert(Pointer_stringify(str));
	console.log(Pointer_stringify(str));
	//hom3r.namespace.publicFunc(Pointer_stringify(str));
	Hom3rAPI.SetInfoFromHom3r(Pointer_stringify(str), "hola");
  },
  SendToConsole: function (str) {    
	console.log(Pointer_stringify(str));	
  },
  SendToApp: function (id, message, value) {    
	//console.log(Pointer_stringify(message));
	//console.log(Pointer_stringify(value));	
	Hom3rAPI.SetInfoFromHom3r(Pointer_stringify(id), Pointer_stringify(message), Pointer_stringify(value));
  },  
  PrintFloatArray: function (array, size) {
    for(var i = 0; i < size; i++)
    console.log(HEAPF32[(array >> 2) + i]);
  },

  AddNumbers: function (x, y) {
    return x + y;
  },

  StringReturnValueFunction: function () {
    var returnStr = "bla";
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },

});