Marketplace.add({
    label:'Device profiles',
    icon:'profiles.jpg',
    add:function(){
        var id = 'deviceDefaultsHost'
        $('#'+id).remove()
        var host = $("<div title='Device default profiles' id='"+id+"'></div>")
        var profiles = 
        [{
            label:'IPhone (3 or 4)',
            icon:'iphone.jpg',
            plugins:["Visual navigation","Quizzing","Visual slide display","Easy Tiling"]
        },
        {
            label:'Tablet PC',
            icon:'tablet.jpg',
            plugins:["Visual navigation","Search","Quizzing","Active ink","Easy Tiling","Visual slide display"]
        },
        {
            label:'Laptop',
            icon:'laptop.jpg',
            plugins:["Visual navigation","Search","Quizzing","Easy Tiling","Visual slide display"]
        }]
        _.each(profiles,function(deviceProfile){
            var profileLauncher = $("<img src='/public/images/"+deviceProfile.icon+"'></img>").click(function(){
                _.each(deviceProfile.plugins, function(plugin){
                   Commands.fire("loadPlugin",plugin) 
                })
            })
            host.append(profileLauncher)
        })
        host.dialog({width:500,height:220})
    }
})
