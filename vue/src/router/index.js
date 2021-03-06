import Vue from 'vue'
import Router from 'vue-router'
import Home from '../views/Home.vue'
import Login from '../views/Login.vue'
import Logout from '../views/Logout.vue'
import Register from '../views/Register.vue'
import store from '../store/index'
import Profile from '../views/Profile.vue'
import CreateTeam from '../views/CreateTeam.vue'
import CreateProject from '../views/CreateProject.vue'
import Team from '../views/Team.vue'
import Project from '../views/Project.vue'
import Search from '../views/Search.vue'
import CreateEvent from '../views/CreateEvent.vue'
import Event from '../views/Event.vue'
import CreateTemplate from '../views/CreateTemplate.vue'

Vue.use(Router)

/**
 * The Vue Router is used to "direct" the browser to render a specific view component
 * inside of App.vue depending on the URL.
 *
 * It also is used to detect whether or not a route requires the user to have first authenticated.
 * If the user has not yet authenticated (and needs to) they are redirected to /login
 * If they have (or don't need to) they're allowed to go about their way.
 */

const router = new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home,
      meta: {
        requiresAuth: true
      }
    },
    {
      path: "/login",
      name: "login",
      component: Login,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/logout",
      name: "logout",
      component: Logout,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/register",
      name: "register",
      component: Register,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/profiles/:userId",
      name: "profiles",
      component: Profile,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/teams/create",
      name: "createteam",
      component: CreateTeam,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/projects/create",
      name: "createproject",
      component: CreateProject,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/projects/:projId",
      name: "project",
      component: Project,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/teams/:teamId",
      name: "team",
      component: Team,
      meta: {
        requiresAuth: false
      }
    },
    {
      path: "/search",
      name: "search",
      component: Search,
      meta: {
        requiresAuth: false
      }
    },
    {
    path: "/projects/:projId/events",
    name: "createevent",
    component: CreateEvent,
    meta: {
      requiresAuth: false
    }
    },

    {
      path: "/projects/:projId/events/:eventId",
      name: "event",
      component: Event,
      meta: {
        requiresAuth: false
      }
      },

      {
        path: "/createtemplate",
        name: "createtemplate",
        component: CreateTemplate,
        meta: {
          requiresAuth: false
        }
        },
  ]
})

router.beforeEach((to, from, next) => {
  // Determine if the route requires Authentication
  const requiresAuth = to.matched.some(x => x.meta.requiresAuth);

  // If it does and they are not logged in, send the user to "/login"
  if (requiresAuth && store.state.token === '') {
    next("/login");
  } else {
    // Else let them go to their next destination
    next();
  }
});

export default router;
