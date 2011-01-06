Marketplace.add({
    label:'Device profiles',
    icon:'profiles.jpg',
    add:function(){
        var id = 'deviceDefaultsHost'
        $('#'+id).remove()
        var host = $("<div title='Device default profiles' id='"+id+"'></div>")
        var profiles = 
        [{
            id:'iPhone',
            label:'IPhone (3 or 4)',
            icon:'iphone.jpg',
            plugins:["Visual navigation","Quizzing","Visual slide display","Easy Tiling"]
        },
        {
            id:'tablet',
            label:'Tablet PC',
            icon:'tablet.jpg',
            plugins:["Easy Tiling","Visual navigation","Search","Quizzing","Active ink","Visual slide display","Author search","Skynet","Analysis"]
        },
        {
            id:'laptop',
            label:'Laptop',
            icon:'laptop.jpg',
            plugins:["Easy Tiling","Visual navigation","Search","Quizzing","Visual slide display","Author search","Skynet","Analysis","Threaded discussion"]
        }]
        host.dialog({width:500,height:220})
        _.each(profiles,function(deviceProfile){
            var profileLauncher = $("<img src='/public/images/"+deviceProfile.icon+"'></img>").click(function(){
                _.each(deviceProfile.plugins, function(plugin){
                   Commands.fire("loadPlugin",plugin) 
                })
            })
            host.append(profileLauncher)
            if(device == deviceProfile.id){
                profileLauncher.trigger('click')
                host.dialog('close')
            }
        })
    }
})
