<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; ?>
 
 <?php 
  // Capture the values posted to this php program from the text fields
 // which were named 'YourName' and 'FavoriteWord' respectively
 
 $yourName=$_REQUEST['yourName'];
 
 $SayStuff=$_REQUEST['SayStuff'];
 ?>
 <p>
 Hi <?php print $yourName; ?>
 <p>
 You said <b> <?php print $SayStuff; ?> !!!</b>
 
 </body>
</html>