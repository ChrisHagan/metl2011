var Marketplace = (function(){
    var launch = $("<div>Launch MeTL Marketplace</div>").css({position:'fixed',top:0,right:0,'z-index':1230,width:'770px'}).click(function(){
        $('#'+id).dialog('open')
    })
    $('body').append(launch)
    var id = "marketplaceHost"
    $('#'+id).remove()
    var host = $("<div id='"+id+"' title='MeTL Marketplace'></div>").dialog({autoOpen:false})
    return {
        add:function(marketable){
            host.append($("<div class='item'><div><img src='/public/images/"+marketable.icon+"' /><div><div>"+marketable.label+"</div></div>").click(function(){
                marketable.add()
            }))
        }
    }
})()
