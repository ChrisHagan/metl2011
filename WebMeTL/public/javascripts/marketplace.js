var Marketplace = (function(){
    var available = []
    var loaded = []
    var launch = $("<div>Launch MeTL Marketplace</div>").css({position:'fixed',top:0,right:0,'z-index':1230}).addClass('ui-widget-header').addClass('ui-corner-all').click(function(){
        $('#'+id).dialog('open')
    })
    $('body').append(launch)
    var id = "marketplaceHost"
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
