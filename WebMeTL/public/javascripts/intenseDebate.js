Marketplace.add({
    label:'Threaded discussion',
    icon:'threads.jpg',
    add:function(){
        var id = "IDCommentsPostTitle"
        var server = "http://radar.adm.monash.edu:9000"
        var display = function(jid){
            //Globals for IntenseDebate
            idcomments_acct = 'eadc7f2a59159b61fc9dffd09fce23ff';
            idcomments_post_id = sprintf("%s/%s",server,jid);
            idcomments_post_url = document.idcomments_post_id;
            if($('#'+id).length == 0){
                $('body')
                    .append($(sprintf("<div id='%s'></div>",id))
                        .append($("<script type='text/javascript' src='http://www.intensedebate.com/js/genericLinkWrapperV2.js'></script>")))
            }
        }
        Commands.add("conversationJoined",function(conversation){
            display(conversation.jid)
        })
    }
})
