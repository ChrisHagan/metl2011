var CommandInserter = function(context){
    var events = []
    var handlers = []
    return{
        add:function(evt,handler){
            if(!(evt in handlers))
                handlers[evt] = []
            handlers[evt].push(handler)
            context[evt] = function(args){
                for(var i = 0;i<handlers[evt].length;i++)
                    //Undefined is false.  Return true ("handled") to stop further handlers
                    if(handlers[evt][i](args))
                        break 
                }
            }
        }
    }
var Commands = CommandInserter(window)
