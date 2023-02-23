import React, { Component } from 'react';

const ConnectedUsers = ({ users }) => <div className='users-container'>
    <h4>Players:</h4>
    <div className='userlist'>
        {users.map((u, idx) => <h6 key={idx}>{u}</h6>)}
    </div>
</div>

export default ConnectedUsers;