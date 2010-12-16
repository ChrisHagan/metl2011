import org.junit._
import org.junit.Assert._
import utils.Stemmer._
import play.test._
class UtilsTest extends UnitTest{
    @Test
    def stemming={
        assertEquals("00",utils.Stemmer.stem("1"))
        assertEquals("00",utils.Stemmer.stem("007"))
        assertEquals("01",utils.Stemmer.stem("1000"))
        assertEquals("10",utils.Stemmer.stem("10000"))
        assertEquals("10",utils.Stemmer.stem("10294"))
        assertEquals("20",utils.Stemmer.stem("10220294"))
    }
    @Test
    def reabsolutizingUris={
        val s = "aserver"
        val `public` = "https://aserver.adm.monash.edu:1188/Resource/12/12345/aresource.anextension"
        val `private` = "https://aserver.adm.monash.edu:1188/Resource/ab/abcde/12345/aresource.anextension"
        assertEquals(`public`, reabsolutizeUri(s,"/Resource/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"/Resource/abcde/12345/aresource.anextension"))
        assertEquals(`public`, reabsolutizeUri(s,"/Resource/12/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`public`, reabsolutizeUri(s,"http://aserver.adm.monash.edu:1188/Resource/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"http://aserver.adm.monash.edu:1188/Resource/abcde/12345/aresource.anextension"))
        assertEquals(`public`, reabsolutizeUri(s,"http://aserver.adm.monash.edu:1188/Resource/12/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"http://aserver.adm.monash.edu:1188/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`public`, reabsolutizeUri(s,"https://aserver.adm.monash.edu:1188/Resource/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"https://aserver.adm.monash.edu:1188/Resource/abcde/12345/aresource.anextension"))
        assertEquals(`public`, reabsolutizeUri(s,"https://aserver.adm.monash.edu:1188/Resource/12/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"https://aserver.adm.monash.edu:1188/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"https://anotherserver.adm.monash.edu:1188/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"https://aserver.adm.monash.edu:1749/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"http://anotherserver.adm.monash.edu:1749/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"http://aserver.adm.monash.edu:1749/Resource/ab/abcde/12345/aresource.anextension"))
        assertEquals(`private`, reabsolutizeUri(s,"https://anotherserver.adm.monash.edu:1749/Resource/ab/abcde/12345/aresource.anextension"))
    }
}
