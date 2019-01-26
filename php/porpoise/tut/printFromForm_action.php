<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>printing data from HTML form</p>'; ?>

Hi <?php echo htmlspecialchars($_POST['name']); ?>.
You are <?php echo (int)$_POST['age']; ?> years old.
 
 </body>
</html>