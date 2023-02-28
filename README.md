# AI-Guise
 COMP 4952 ASP Web App Extension

changes: 
day 0 - deployed website, nothing is server instanced and all users view and share the same game

day 1 - tried to fix the Firestore database doc limit by using Firebase Cloud storage, couldn't figure out how to properly generate to URL 
and couldn't solve the 400 error

day 2 - fixed issue with multiple repeating words not showing up properly, fixed issue of db doc size limit for now (split images into 4 seperate byte strings),
 added returning the doc id after creating a game to instantly play it

day 3 - essentially fixed the multiple user issue by adding session state variables instead of a static variable in controllers

day 4 - everything is stored in sessions now

day 5 and 6ish - fixed a bunch of css

day 7 - css hell, still have game page and instructions page

day 8 - more css

day 9 - all pages are responsive , added some functionality


TODO:
- fix the server and client issues (done)
- make website responsive (done for now)
- add ?O tutorial (like wordle)
- remove difficulties, only play existing game and new game +
- make website logo and icon
- improve buttons
- add random prompt generator
- add loading screen on and improve home page
- make reloading the /game page work properly (not supposed to load new game, go back to instructions page)
- make trying to enter a word after session expire redirect to home page (check for empty session game)
- add "already guessed" dialog when already entering a word 
- check for invalid letters and symbols
LOW PRIO:
- add share 
- add game pages for each game in URL