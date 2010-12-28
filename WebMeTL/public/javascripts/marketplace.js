var Marketplace = (function(){
    var available = []
    var loaded = []
    var id = "marketplaceHost"
    $("#marketplaceLauncher").click(function(){
        $('#'+id).dialog('open')
    })
    $('#'+id).remove()
    var host = $("<div id='"+id+"' title='MeTL Marketplace'></div>").dialog({width:520,minHeight:600,position:['left','top'],autoOpen:false})
    Commands.add("loadPlugin",function(param){
        var marketable
        if(typeof param == "string"){
            var matches = _.filter(available,function(plugin){
                return plugin.label == param
            })
            marketable = matches[0]
        }
        else marketable = param
        if(_.indexOf(marketable,loaded) == -1){
            marketable.add()
            loaded.push(marketable)
            host.find("div.item:contains('"+marketable.label+"')").css("opacity",0.2)
        }
    })
    return {
        add:function(marketable){
            available.push(marketable)
            host.append($("<div class='item'><div><img src='/public/images/"+marketable.icon+"' /><div><div>"+marketable.label+"</div></div>").click(function(){
                Commands.fire("loadPlugin",marketable)
            }))
        },
        loaded:loaded
    }
})()
