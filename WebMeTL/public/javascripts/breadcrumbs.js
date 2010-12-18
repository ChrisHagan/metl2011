var Breadcrumb = (function(){
    var breadcrumbHost = $("<div id='breadcrumbHost' title='Breadcrumbs'></div>")
    $('body').append(breadcrumbHost)
    breadcrumbHost.dialog({minHeight:75, position:['left','top']})
    Commands.add("conversationJoined",function(conversation){
        Breadcrumb.add(conversation.title, function(){
            conversationJoined(conversation)
        })
    })
    var trail = []
    return {
        add: function(label, func){
            if(_.contains(_.pluck(trail, "label"), label)) return;
            trail.push({label:label, func:func})
            this.render()
        },
        render:function(){
            breadcrumbHost.html("")
            var recentCrumbs = [{label:"All authors", func:searchForConversations}]
            _.each(trail.slice(-10), function(crumb){recentCrumbs.push(crumb)})
            _.each(recentCrumbs, function(crumb){
                breadcrumbHost.append($("<span />").append(crumb.label.slice(0,15) + "->").click(function(){
                    scroll(0,0)
                    crumb.func()
                }))
            })
        }
    }
})()
