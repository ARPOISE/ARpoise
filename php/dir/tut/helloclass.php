<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; ?>
 
 <?php
 class Dictionary {
 	public $translations = array();
 	public $type ="En";
 }
 $dic_de = new Dictionary();
 $dic_de ->type = "De" ;
 $dic_de ->translations['tree'] = "Baum" ;

 $dic_fr = new Dictionary();
 $dic_fr ->type = "Fr" ;
 $dic_fr ->translations['tree'] = "arbre" ;
 
 foreach (array($dic_de, $dic_fr) as $dictionary){
 	print "type: {$dictionary->type} " ;
 	print "tree: {$dictionary->translations['tree']}<br/>" ;
 }
 ?>
 </body>
</html>