<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; ?>
 
 <?php
 
  $men=array("Tom", "Dick", "Harry"); // array 0,1,2
  $men[]="Juan";
  print "$men[3]";
 
  $firstMan=array(
				name=>"Jeff",
				description=>"nerd",
				haircolor=>"black",
				age=>17
				);
 print ("<p>My first boyfriend was $firstMan[age] old");
 
 ?>
 
 </body>
</html>