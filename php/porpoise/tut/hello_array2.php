<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; ?>
 
 <?php
 
  $men=array(
			 
  		array(  name=>"Jeff",
				description=>"electronics whiz",
				haircolor=>"black, straight",
				hisage=>17,
				myage=>15),
  
    	array(  name=>"David",
				description=>"foreign service",
				haircolor=>"black, wavy",
				hisage=>21,
				myage=>17),
			 
    	array(  name=>"Tom",
				description=>"aero astro",
				haircolor=>"brown, curly",
				hisage=>22,
				myage=>18),
  );
 

 print ("<p>My second boyfriend was ");
 print  $men[1]["name"];
 print  (", he was ");
 print  $men[1]["hisage"];
 print  (" years old");
 
 ?>
 
 </body>
</html>