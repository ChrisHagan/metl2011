Marketplace.add({
    label:'Easy Tiling',
    icon:'tiling.jpg',
    add:function(){
       $('.ui-dialog-titlebar').live('dblclick',function(){
           var expansionTarget = $(this).siblings('.ui-dialog-content')//The original dialog source
           expansionTarget.dialog({width:width,position:['left','top']})
           expansionTarget.parent('.ui-dialog').css({'z-index':0})//Now that we're maximized, drop back
       })
    }
})
