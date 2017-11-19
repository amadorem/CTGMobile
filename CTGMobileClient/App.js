import React, { Component } from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  TouchableOpacity,
  View
} from 'react-native';
import {ReactNativeAD, ADLoginView} from 'react-native-azure-ad'

const CLIENT_ID = '7d825819-1c1b-4626-8453-a0879c4374e4'
const AUTH_URL = 'https://login.microsoftonline.com/common/oauth2/authorize'

const ADContext = new ReactNativeAD({
      client_id : CLIENT_ID,
      redirectUrl : 'https://localhost:44365',
      authority_host : AUTH_URL,
      tenant  : '0938e32f-3ac8-42db-8afc-037070df5145',
      client_secret : 'MPfQ0TgD8PWcFLWGHLDngluS3geO9J36Ibz3u6OGU8k=',
      resources : [
        'https://graph.microsoft.com',
      ]
    })

export default class App extends Component {
  constructor(props) {
    super(props)
    this.state = {
      info : null,
      shouldLogout : false,
      displayType : 'before_login'
    }
  }

  render() {
    return (
      <View style={styles.container}>
        {this._renderContent.bind(this)()}
      </View>
    );
  }

  _renderContent() {

    switch(this.state.displayType) {
      case 'before_login' :
        return <TouchableOpacity style={styles.button}
          onPress={(this._showADLogin.bind(this))}>
          <Text style={{color : 'white'}}>Login</Text>
        </TouchableOpacity>

      case 'login' :  
        return [
          <ADLoginView
            key="webview"
            hideAfterLogin={true}
            style={{flex :1}}
            needLogout={this.state.shouldLogout}
            context={ADContext}
            onURLChange={this._onURLChange.bind(this)}
            onSuccess={this._onLoginSuccess.bind(this)}/>]
      case 'after_login' :
        return [
          <Text key="text">You're logged in as {this.state.info} </Text>,

          <TouchableOpacity key="button" style={styles.button}
            onPress={(this._logout.bind(this))}>
            <Text style={{color : 'white'}}>Logout</Text>
          </TouchableOpacity>]
      break
    }
  }

  _onURLChange(e) {
    let isLoginPage = e.url === `${AUTH_URL}?response_type=code&client_id=${CLIENT_ID}`

    if(isLoginPage && this.state.shouldLogout) {
      console.log('logged out')
      this.setState({
        displayType : 'before_login',
        shouldLogout : false
      })
    }
  }

  _showADLogin() {
    this.setState({
      displayType : 'login'
    })
  }

  _logout() {
    this.setState({
      displayType : 'login',
      shouldLogout : true
    })
  }

  _onLoginSuccess(cred) {
    console.log('user credential', cred)

    let access_token = ADContext.getAccessToken('https://graph.microsoft.com')
    fetch('https://graph.microsoft.com/beta/me', {
      method : 'GET',
      headers : {
        Authorization : `bearer ${access_token}`
      }
    })
    .then(res => res.json())
    .then(user => {
      console.log(user)
      this.setState({
        displayType : 'after_login',
        info : user.displayName
      })
    })
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F5FCFF',
  },

  button : {
    margin : 24,
    backgroundColor : '#1a6ed1',
    padding : 12
  },
});