Marketplace.add({
    label:'Threaded discussion',
    icon:'threads.jpg',
    add:function(){
        var id = "idc-container"
        var server = "http://radar.adm.monash.edu:9000"
        //Global
        idcomments_acct = 'eadc7f2a59159b61fc9dffd09fce23ff';
        var display = function(jid){
            //Globals for IntenseDebate
            idcomments_post_id = sprintf("%s/%s",server,jid);
            idcomments_post_url = idcomments_post_id;
            $('#'+id).remove()
            $('body').append($(sprintf("<div id='%s'></div>",id)))
            $.getScript('http://www.intensedebate.com/js/genericCommentWrapperV2.js')
        }
        Commands.add("conversationJoined",function(conversation){
            display(conversation.jid)
        })
    }
})
