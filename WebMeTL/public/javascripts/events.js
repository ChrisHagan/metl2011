var Commands = function(context){
    var events = []
    var handlers = []
    _.each(events,function(acc,evt){
        window[evt] = function(args){
            if(evt in handlers)
                for(var i = 0;i<handlers[evt].length;i++)
                    if(!handlers[evt][i](args))
                        break }})
    return
    {
        add:function(evt,func){
            if(!evt in handlers)
                handlers[evt] = []
            handlers[evt].push(func)
        }
    }
}
