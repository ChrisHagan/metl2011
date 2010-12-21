(function($){
    jQuery.fn.bubble = function(){
        var pad = 50
        this.addClass('bubble')
        this.css({
            'border':'1px solid black',
            'top':0,
            'left':0,
            'padding':''+pad+'px',
            'z-index':1030,
            'background-color':'white'
        })
        this.corner((pad * 2)+"px")
        this.draggable({revert:'invalid'})
        return this
    }
})(jQuery)
