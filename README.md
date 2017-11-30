# netchat
A simple network chat application implemented with multicast 

Requirements are:

  Router must support multicast (UDP)
  Same subnet e.g. 
  
  - A: 10.0.2.15
    
  - B: 10.0.2.10
    
  - C: 10.0.3.5
    
    -> A, B can chat. C cannot chat
  
Details of communication:

  - Multicast IP: 224.0.0.3
  
  - Multicast Port: 80

Built using C#
