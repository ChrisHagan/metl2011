var Marketplace = (function(){
    var launch = $("<div>Launch MeTL Marketplace</div>").css({position:'fixed',top:0,right:0,'z-index':1230}).addClass('ui-widget-header').addClass('ui-corner-all').click(function(){
        $('#'+id).dialog('open')
    })
    $('body').append(launch)
    var id = "marketplaceHost"
    $('#'+id).remove()
    var host = $("<div id='"+id+"' title='MeTL Marketplace'></div>").dialog({width:500,height:600,position:['left','top']})
    return {
        add:function(marketable){
            host.append($("<div class='item'><div><img src='/public/images/"+marketable.icon+"' /><div><div>"+marketable.label+"</div></div>").click(function(){
                $(this).css({"background-color":"green","opacity":"0.3"}).click(function(){alert('You have already loaded the '+marketable.label+' plugin')})
                marketable.add()
            }))
        }
    }
})()
