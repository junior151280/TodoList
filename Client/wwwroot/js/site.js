// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const person = document.querySelector('.my-person-card');
const personCard = document.querySelector('mgt-person-card');

person.personDetails = {
    displayName: 'Jailton S. Sales Jr',
    mail: 'jailtons@microsoft.com'
};
person.personImage = '../images/profile.jpg';

personCard.addEventListener('expanded', () => {
    console.log("expanded");
})
