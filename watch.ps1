do{

    $CURRENT_BRANCH=(git symbolic-ref --short -q HEAD) 

    git pull origin "$CURRENT_BRANCH"; 
    git add .; 
    git commit -m 'Working...'; 
    git push origin "$CURRENT_BRANCH";

}while($true)
