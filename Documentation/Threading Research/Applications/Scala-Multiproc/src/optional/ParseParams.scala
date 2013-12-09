/*
*TITLE: 	Scala open source utility to automagically parser parameters.
*AUTOHR: 	Not me. 
*AUTHOR URL:https://code.google.com/p/parse-cmd/wiki/AScalaParserClass
*
*PURPOSE: 	To help fascilitate parsing unix-style input paramters 
* 			in Scala.
*
 */
package optional

class ParseParms(val help: String) {

    private var parms = Map[String,(String,String,Boolean)]()
    private var cache: Option[String] = None    // save parm name across calls
                                                // used by req and rex methods
    def parm(name: String) = {
        parms += name -> ("", "^.*$", false ) ;cache = Some(name)
        this
    }

    def parm(name: String, default: String) = {
        parms += name -> (default, defRex(default), false); cache = Some(name)
        this
    }

    def parm(name: String, default: String, rex: String) = {
        parms += name -> (default, rex, false); cache = Some(name)
        this
    }

    def parm(name: String, default: String, rex: String, req: Boolean) = {
        parms += name -> (default, rex, req); cache = Some(name)
        this
    }

    def parm(name: String, default: String, req: Boolean) = {
        parms += name -> (default, defRex(default), req); cache =  Some(name)
        this
    }

    def req(value: Boolean) = {                 // update required flag
        val k = checkName                       // for current parameter name
        if( k.length > 0 ) {                    // stored in cache
            val pvalue = parms(k)               // parmeter tuple value
            val ntuple = (pvalue._1,pvalue._2,value)    // new tuple
            parms += cache.get -> ntuple        // update entry in parms
        }                                       // .parm("-p1","1").req(true)
        this                                    // enables chained calls
    }

    def rex(value: String) = {                  // update regular-expression
        val k = checkName                       // for current name
        if( k.length > 0 ) {                    // stored in cache
            val pvalue = parms(k)               // parameter tuple value
            val ntuple = (pvalue._1,value,pvalue._3)    // new tuple
            parms += cache.get -> ntuple        // update tuple for key in parms
        }                                       // .parm("-p1","1").rex(".+")
        this                                    // enables chained calls
    }

    private def checkName = {                           // checks name stored in cache
        cache match {                           // to be a parm-name used for
            case Some(key) => key               // req and rex methods
            case _         => ""                // req & rex will not update
        }                                       // entries if cache other than
    }                                           // Some(key)

    private def defRex(default: String): String = {
        if( default.matches("^\\d+$") ) "^\\d+$" else "^.*$"
    }

    private def genMap(args: List[String] ) = { // return a Map of args
        var argsMap = Map[String,String]()      // result object
        if( ( args.length % 2 ) != 0 ) argsMap  // must have pairs: -name value
        else {                                  // to return a valid Map
            for( i <- 0.until(args.length,2) ){ // iterate through args by 2
                argsMap += args(i) -> args(i+1) // add -name value pair
            }
            argsMap                             // return -name value Map
        }
    }

    private def testRequired( args: Map[String,String] ) = {
        val ParmsNotSupplied = new collection.mutable.ListBuffer[String]
        for{ (key,value) <- parms               // iterate trough parms
            if value._3                         // if parm is required
            if !args.contains(key)              // and it is not in args
        } ParmsNotSupplied += key               // add it to List
        ParmsNotSupplied.toList                 // empty: all required present
    }

    private def validParms( args: Map[String,String] ) = {
        val invalidParms = new collection.mutable.ListBuffer[String]
        for{ (key,value) <- args                // iterate through args
            if parms.contains(key)              // if it is a defined parm
            rex = parms(key)._2                 // parm defined rex
            if !value.matches(rex)              // if regex does not match
        } invalidParms += key                   // add invalid arg
        invalidParms.toList                     // empty: all parms valid
    }

    private def mergeParms( args: Map[String,String] ) = {
        //val mergedMap = collection.mutable.Map[String,String]()
        var mergedMap = Map[String,String]()    // name value Map of results
        for{ (key,value) <- parms               // iterate through parms
            //mValue = if( args.contains(key) ) args(key) else value(0)
            mValue = args.getOrElse(key,value._1)  // args(key) or default
        }   mergedMap +=  key -> mValue         // update result Map
        mergedMap                               // return mergedMap
    }

    private def mkString(l1: List[String],l2: List[String]) = {
        "\nhelp:   " + help + "\n\trequired parms missing: "  +
        ( if( !l1.isEmpty ) l1.mkString(" ")  else "" )       +
        ( if( !l2.isEmpty ) "\n\tinvalid parms:          "    +
               l2.mkString(" ") + "\n" else "" )
    }

    def validate( args: List[String] ) = {          // validate args to parms
        val argsMap   = genMap( args )              // Map of args: -name value
        val reqList   = testRequired( argsMap )     // List of missing required
        val validList = validParms( argsMap )       // List of (in)valid args
        if( reqList.isEmpty && validList.isEmpty ) {// successful return
            (true,"",mergeParms( argsMap ))         // true, "", mergedParms
        } else (false,mkString(reqList,validList),Map[String,String]())
    }
}
