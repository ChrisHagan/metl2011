Marketplace.add({
    label:'Loaded plugins',
    icon:'plugins.jpg',
    add:function(){
        var id = 'loadedPluginsHost'
        var host = $(sprintf("<div id='%s' title='Loaded plugins'></div>", id)).dialog()
        var displayLoaded = function(){
            _.each(Marketplace.loaded, function(param){
                var label
                if(typeof param == "string")
                    label = param
                else
                    label = param.label
                host.append($(sprintf("<span>%s </span>",label)))
            })
        }
        displayLoaded()
        Commands.add('loadPlugin',displayLoaded)
    }
})
