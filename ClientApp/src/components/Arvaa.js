//Vuoron vaihto kun arvaisten määrä täyttyy

//mahdollisuus jos sama sana tulee kaksi kertaa niin piirtäjä pystyy arvaamaan

//Koodin yleissiisteys & ylimääräiset pois

import React, { useState, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import Lobby from './Lobby';
import Chat from './Chat';
import { CirclePicker } from 'react-color';
import { BiBrushAlt } from "react-icons/bi"
import { BsFillEraserFill } from "react-icons/bs"
import { AiOutlineArrowLeft, AiFillDelete } from "react-icons/ai"
import { ImPen } from "react-icons/im"
import { GoPrimitiveDot } from "react-icons/go"
import './Draw.css';
import './Arvaa.css';
import { useDetectOutsideClick } from "./useDetectOutsideClick";
import LZString from 'lz-string';
import CanvasDraw from 'react-canvas-draw';


const Arvaa = () => {
    const [connection, setConnection] = useState();
    const [messages, setMessages] = useState([]);
    const [users, setUsers] = useState([]);
    const [sana, setSana] = useState([]);
    const [color, setColor] = useState('#000000');
    const [brushRadius, setbrushRadius] = useState(8);
    const [userRole, setUserRole] = useState(false);
    const [piirtajanSana, setPiirtajanSana] = useState();
    const [gameWinner, setGameWinner] = useState();

    const colordropdownRef = useRef(null);
    const [isColorActive, setIsColorActive] = useDetectOutsideClick(colordropdownRef, false);
    const onColorClick = () => setIsColorActive(!isColorActive);
    const thicdropdownRef = useRef(null);
    const [isThicActive, setIsThicActive] = useDetectOutsideClick(thicdropdownRef, false);
    const onThicClick = () => setIsThicActive(!isThicActive);
    const colorArray = ["#f44336", "#e91e63", "#9c27b0", "#673ab7", "#3f51b5", "#2196f3", "#000000", "#00bcd4", "#009688", "#4caf50", "#8bc34a", "#cddc39", "#ffeb3b", "#ffc107", "#ff9800", "#ff5722", "#795548", "#607d8b"]

    const joinRoom = async (user, room, createOrJoin) => {
        try {
            
        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:5001/arvaa")
            .configureLogging(LogLevel.Information)
            .build();

      connection.on("ReceiveMessage", (user, message, win) => {
        setMessages(messages => [...messages, { user, message, win }]);
      });

        connection.on("ReceiveData", (canvasData) => {
            console.log(canvasData);
            loadableCanvas.current.loadSaveData(
                LZString.decompressFromBase64(canvasData),
                true
            );
        });
            connection.on("ReceiveWord", (sana) => {
                /*console.log(sana);*/                
                setPiirtajanSana(sana);
            ;
        });
            connection.on("ReceiveAnswer", (answer) => {
                setMessages([]);
                setGameWinner();
                setSana(answer);
            });

      connection.on("UsersInRoom", (users) => {
          setUsers(users);    
      });       

        connection.on("ReceiveIsDrawer", (isDrawer) => {
            setUserRole(isDrawer);
            saveableCanvas.current.eraseAll();
        });

        connection.on("GameWon", (gameWinner) => {
            setGameWinner(gameWinner);               
            });

      connection.onclose(e => {
        setConnection();
        setMessages([]);
          setUsers([]);
          setSana([]);
          setUserRole();
          setGameWinner();
      });

        await connection.start();
            if (createOrJoin == "create") {
            await connection.invoke("CreateRoom", { user, room });
            setUserRole(true);
        }
            if (createOrJoin == "join") {
            await connection.invoke("JoinRoom", { user, room });
            
        }
      
      setConnection(connection);
    } catch (e) {
      console.log(e);
    }
  }

  const sendMessage = async (message) => {
    try {
      await connection.invoke("SendMessage", message);
    } catch (e) {
        console.log(e);
    }
    }

    const sendCanvasData = async (canvasData) => {
        try {
            await connection.invoke("SendCanvasData", LZString.compressToBase64(JSON.stringify(JSON.parse(canvasData, (key, value) => typeof value === "number" ? +value.toFixed(0) : value))));
        } catch (e) {
            console.log(e);
        }
    }
    const closeConnection = async () => {       
        try {
        await connection.stop();            
    } catch (e) {
      console.log(e);
    }
    }
    const saveableCanvas = useRef(null);
    const loadableCanvas = useRef(null);


    function WindowWidth() {

        if (window.innerWidth >= 1070) {
            return(600);            
        }
        if (window.innerWidth >= 600 && window.innerWidth <= 1070) {
            return (window.innerWidth * 0.5);
        }
        else {         
            return(window.innerWidth);            
        }
    }
  return <div className='app'>
      {!connection
          ? <Lobby joinRoom={joinRoom} /> : <>     
              <div className='outer'>                 
                  <Chat className='inner' sendMessage={sendMessage} messages={messages} users={users} closeConnection={closeConnection} piirtajanSana={piirtajanSana} sana={sana} userRole={userRole} gameWinner={gameWinner} />
                  <div className='inner_canvas'>
                      {userRole ? <>
                          
                      <CanvasDraw
                            className="canvas"
                            canvasWidth={WindowWidth()}
                            style={{ zIndex: '3' }}
                            brushColor={color}
                            brushRadius={brushRadius}
                            lazyRadius={5}
                            hideGrid
                            ref={saveableCanvas}
                            onChange={() => {
                            sendCanvasData(saveableCanvas.current.getSaveData());
                            }}
                      />

                      <CanvasDraw
                            className='canvas'
                            canvasWidth={WindowWidth()}
                            disabled
                            hideGrid
                            style={{ zIndex: '1' }}
                            ref={loadableCanvas}
                       />

                          <div className="toolBar">
                              <div className="tools">


                                  <button onClick={onColorClick} className="colorpick-trigger">
                                      <BiBrushAlt size={36} />
                                  </button>
                                  <nav ref={colordropdownRef} className={`colorpick ${isColorActive ? 'active' : 'inactive'}`}>
                                      <ul>
                                          <CirclePicker
                                              colors={colorArray}
                                              color={color}
                                              onChangeComplete={color => {
                                                  setColor(color.hex);
                                              }}
                                              onChange={onColorClick}
                                          />
                                      </ul>
                                  </nav>


                                  <button onClick={onThicClick} className="colorpick-trigger">
                                      <ImPen size={36} />
                                  </button>
                                  <nav ref={thicdropdownRef} className={`colorpick ${isThicActive ? 'active' : 'inactive'}`}>
                                      <ul className="thic-container">
                                          <button onClick={() => { setbrushRadius(16); onThicClick(); }} className="colorpick-trigger">
                                              <GoPrimitiveDot size={56} style={{ margin: "-13px" }} />
                                          </button>
                                          <button onClick={() => { setbrushRadius(8); onThicClick(); }} className="colorpick-trigger">
                                              <GoPrimitiveDot size={42} style={{ margin: "-6px" }} />
                                          </button>
                                          <button onClick={() => { setbrushRadius(4); onThicClick(); }} className="colorpick-trigger">
                                              <GoPrimitiveDot size={18} style={{ margin: "5px" }} />
                                          </button>
                                      </ul>
                                  </nav>

                                  <button className="colorpick-trigger" onClick={() => { setColor('#ffffff'); }}>
                                      <BsFillEraserFill size={36} />
                                  </button>

                                  <button className="colorpick-trigger" onClick={() => { saveableCanvas.current.undo(); }}>
                                      <AiOutlineArrowLeft size={36} />
                                  </button>


                                  <button className="colorpick-trigger" onClick={() => { saveableCanvas.current.eraseAll(); }}>
                                      <AiFillDelete size={36} />
                                  </button>

                              </div>
                          </div>

                      </>
                          :
                     <>                     
                     <CanvasDraw
                      className='canvas'
                      canvasWidth={WindowWidth()}
                      disabled
                      hideGrid
                      ref={loadableCanvas}
                     />
              </>
              }              
                  </div>
              </div>

          </>}
  </div>
}

export default Arvaa;
