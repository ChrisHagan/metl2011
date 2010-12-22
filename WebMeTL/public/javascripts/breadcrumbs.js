Marketplace.add({
    add:function(){
        var trail = []
        var Breadcrumb = {
            add: function(label, func){
                if(_.contains(_.pluck(trail, "label"), label)) return;
                trail.push({label:label, func:func})
                this.render()
            },
            render:function(){
                breadcrumbHost.html("")
                var recentCrumbs = [{label:"All authors", func:Commands.fire.bind('searchForConversations')}]
                _.each(trail.slice(-10), function(crumb){recentCrumbs.push(crumb)})
                _.each(recentCrumbs, function(crumb){
                    breadcrumbHost.append($("<span />").append(crumb.label.slice(0,15) + "->").click(function(){
                        scroll(0,0)
                        crumb.func()
                    }))
                })
            }
        }
        var id = "breadcrumbHost"
        $('#'+id).remove()
        var breadcrumbHost = $("<div id='"+id+"' title='Breadcrumbs'></div>")
        $('body').append(breadcrumbHost)
        breadcrumbHost.dialog({minHeight:75, position:['left','top']})
        Commands.add("conversationJoined",function(conversation){
            Breadcrumb.add(conversation.title, function(){
                Commands.fire('conversationJoined',conversation)
            })
        })
    },
    label:"Breadcrumbs",
    icon:"breadcrumbs.jpg"
})
