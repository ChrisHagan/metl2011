Marketplace.add(
    {
        label:"Just type",
        icon:"justType.jpg",
        add:function(){
            var eliza = new ElizaBot()
            var id = "postItsHost"
            var keys = ""
            var input = $("<div></div>")
            var submit = $("<submit>Submit</submit>")
                .click(function(){
                   bubble({text:keys})
                   keys = ""
                   input.html(keys)
                   dialog.dialog("close")
                })
            var dialog = $("<div id='"+id+"' title='Quick note'></div>")
            dialog.append(input)    
            dialog.append(submit)
            dialog.dialog({ autoOpen:false })

            $(document).keypress(function(event){
                switch(event.which){
                    case 13 ://Enter
                        if(keys.length > 0)
                            $("#"+id+" submit").click(); 
                        else
                            dialog.dialog("close")
                        break;
                    case 32 ://Space
                        if(keys.length > 0){
                            keys += " "
                            event.preventDefault()
                            event.stopPropagation()
                            return false;
                        }
                        break;
                    default : {
                        dialog.dialog("open")
                        keys += String.fromCharCode(event.which)
                        input.html(keys) 
                    }
                }
            })
            $(document).keyup(function(event){
                if(event.which == 8){ //Backspace.  Doesn't get caught by keypress
                    event.preventDefault()
                    event.stopPropagation()
                    if(keys.length > 0){
                        keys = keys.substring(0,keys.length - 1)
                        input.html(keys)
                    }
                    return false;
                }
            })
    }
})
