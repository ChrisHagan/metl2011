Marketplace.add({
    label:'Easy Tiling',
    icon:'tiling.jpg',
    add:function(){
       $('.ui-dialog-titlebar').live('dblclick',function(){
           var width = window.innerWidth
           var height = window.innerHeight
           var expansionTarget = $(this).siblings('.ui-dialog-content')//The original dialog source
           var dialogs = $('.ui-dialog-content')
           var smallDialogHeight = height / (dialogs.length - 1)
           var smallDialogWidth = (width / 6)-30 //Sure, why not?
           expansionTarget.dialog({width:width-smallDialogWidth,height:height,position:[0,0]}).css('z-index',0)
           var yOffset = 0
           dialogs.each(function(){
               if(this != expansionTarget[0]){
                   $(this).dialog({
                       width:smallDialogWidth,
                       height:smallDialogHeight,
                       position:[width - smallDialogWidth,yOffset]
                   })
                   yOffset += smallDialogHeight
               }
           })
        })
    }
})
