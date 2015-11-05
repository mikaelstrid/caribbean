//:#251: http://ckeditor.com/forums/CKEditor-3.x/Disable-Enter-Key
(function ()
{
   var doNothingCmd =
   {
      exec : function( editor )
      {
         return;
      }
   };
   var pluginName = 'doNothing';
   CKEDITOR.plugins.add( pluginName,
   {
      init : function( editor )
      {
         editor.addCommand( pluginName, doNothingCmd );
      }
   });
})();