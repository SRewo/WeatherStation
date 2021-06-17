# WeatherStation
WeatherStation is a .NET project focused on providing accurate weather forecasts.

## Table of Contents
* [Technologies](#technologies)
* [Features](#features)
* [Project Structure](#project-structure)
* [Installation](#installation)
* [List of used APIs](#list-of-used-apis)

## Technologies
Project is created with:
* .NET Standard 2.0
* Xamarin.Forms
* RestSharp
* Microcharts
* Xunit
* Autofac.Extras.Moq
* Prism 8

## Features
  - Display current weather, hourly and daily forecast for selected weather APIs
  - Change city for weather data
### To do:
  - Add more weather APIs
  - Add weather icons
  - Add support for displaying data from selected weather sensors
  - Add way to save and store data from those sensors
  - Add way to compare those data with data downloaded from implemented APIs
  - Add in app recommendation for the most accurate weather API

## Project Structure
Project is composed of:
 * WeatherStation.Library - contains classes for downloading data from repositiories
 * WeatherStation.App - contains Xamarin.Forms projects for displaying data
 * WeatherStation.x.Tests - contains unit tests for x 

## Installation
All used API keys are stored in static partial class AppApiKeys, which has to be defined with valid api keys before project building.

## List of used APIs

### Geolocation
* [Positionstack](https://positionstack.com/)
### Weather
* [AccuWeather](https://developer.accuweather.com/)
