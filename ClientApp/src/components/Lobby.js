import React, { useState } from 'react';

const Lobby = ({ joinRoom }) => {
    const [user, setUser] = useState();
    const [room, setRoom] = useState();
    const [createOrJoin, setCreateOrJoin] = useState();

    return (
        <form className="Home"
            onSubmit={e => {
                e.preventDefault();
                joinRoom(user, room, createOrJoin);                
            }}>
            <h1>Tervetuloa tauolle!</h1>
            <input type="text" placeholder="name" maxLength="10" onChange={e => setUser(e.target.value)} />
            <input type="text" placeholder="room" maxLength="10" onChange={e => setRoom(e.target.value)} />
            <button type="submit" onClick={e => setCreateOrJoin("join")} disabled={!user || !room}><p>Join</p></button>
            <button type="submit" onClick={e => setCreateOrJoin("create")} disabled={!user || !room}><p>Create Game</p></button>
        </form>);
}

export default Lobby;
