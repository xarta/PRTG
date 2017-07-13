# TO DO
My WordPress blogs run on both IIS, Windows 10, and Apache, Ubuntu.

PRTG provides a mini-probe for Linux in Python - fairly limited.  What I'd like to do though, is have a common .Net Core "sensor" that can, in this case, check for my own WordPress back-up script back-up files on a daily basis.  Moving forward, in the future, I want to try and do as much as possible in a RESTful way, isolating vlans as much as possible.

This particular "sensor" / WebAPI will provide JSON, initially with a minimum entry of the filesize found for a named file (0 if error etc.) No authentication is required or anything fancy.  It will be internal use only, still over SSL of course, but on a different, free port (haven't chosen yet).

PRTG already provide "advanced" http sensors that can check JSON in a response for a value ... but in the future I could make my own custom http sensor to parse JSON, and depending on parameters passed to that sensor, provide in it's response only a selected channel of data.

That's the plan anyway.

About time I got back into .Net and learn how to do WebAPI in .Net Core, and, in linux.
I'll be learning as I go along - with nearly everything I do, as I go along, probably turning-out to be a mistake.
