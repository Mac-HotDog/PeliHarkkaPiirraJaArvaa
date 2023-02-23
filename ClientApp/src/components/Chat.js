import SendMessageForm from './SendMessageForm';
import MessageContainer from './MessageContainer';
import ConnectedUsers from './ConnectedUsers';
import Guess from './Guess';
import './Chat.css';
import React, { Component } from 'react';


const Chat = ({ sendMessage, messages, users, closeConnection, sana, userRole, piirtajanSana, gameWinner }) => <div>
    {/*<div className='user-list'>*/}
    {/*    <button className='leave-room' onClick={() => closeConnection()}>Leave Room</button>*/}
    {/*    <ConnectedUsers users={users} />*/}
        
    {/*</div>*/}
    
    {/*<div className='chat'>*/}
    {/*    <MessageContainer className='message-container' messages={messages} />*/}
    {/*    <SendMessageForm sendMessage={sendMessage} />*/}
    {/*</div>*/}



    <div className='box'>
        <div className='top'>
            <div className='top_in'> <button className='leave-room' onClick={() => closeConnection()}>Leave Room</button></div>
            <div className='top_in-users'> <ConnectedUsers className="user-container" users={users} /></div>
            <div className='top_in'> <MessageContainer className='message-container' messages={messages} sana={sana} gameWinner={gameWinner} userRole={userRole} /></div>
        </div>

        <div className='middle'>
            {userRole ? <div className='middle_guess'> <Guess className='guessword-container' sana={piirtajanSana} /></div>
                : <div className='middle_guess'><h5>Your turn to Guess.</h5></div>
            }
        </div>

        <div className='bottom'>
            <SendMessageForm  sendMessage={sendMessage} />
        </div>
    </div>

</div>

export default Chat;