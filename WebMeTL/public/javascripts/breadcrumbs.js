var Breadcrumb = (function(){
    var breadcrumbHost = $("<div id='breadcrumbHost'>Breadcrumbs go here</div>")
    $('body').append(breadcrumbHost)
    breadcrumbHost.dialog()
    var trail = []
    return {
        add: function(label, func){
            if(_.contains(_.pluck(trail, "label"), label)) return;
            trail.push({label:label, func:func})
            this.render()
        },
        render:function(){
            breadcrumbHost.html("")
            var recentCrumbs = [{label:"All authors", func:Authors.radial}]
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
